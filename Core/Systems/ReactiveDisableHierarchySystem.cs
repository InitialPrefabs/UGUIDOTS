using System.Runtime.CompilerServices;
using UGUIDots.Render;
using UGUIDots.Render.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace UGUIDots.Transforms.Systems {

    /// <summary>
    /// Forces an update of the children entities when the parent entity has a disabled tag.
    /// </summary>
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    [UpdateBefore(typeof(UpdateMeshSliceSystem))]
    public class ReactiveDisableHierarchySystem : SystemBase {

        private struct ZeroAlphaJob {

            public EntityCommandBuffer CmdBuffer;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            public BufferFromEntity<LocalVertexData> LocalVertices;

            public void Execute(Entity entity) {
                // CmdBuffer.RemoveComponent<DisableRenderingTag>(entity);
                // if (Parents.Exists(entity)) {
                //     var root = HierarchyUtils.GetRoot(entity, Parents);
                //     CmdBuffer.AddComponent<UpdateVertexColorTag>(root);
                // } else {
                //     // We know that this is the root
                //     CmdBuffer.AddComponent<UpdateVertexColorTag>(entity);
                // }

                if (LocalVertices.Exists(entity)) {
                    var vertices = LocalVertices[entity].AsNativeArray();
                    UpdateVertices(vertices);
                }

                if (Children.Exists(entity)) {
                    var children = Children[entity].AsNativeArray();

                    UpdateAlphaOfChildren(children);
                }
            }

            public void UpdateAlphaOfChildren(NativeArray<Child> children) {
                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    if (LocalVertices.Exists(child)) {
                        var vertices = LocalVertices[child].AsNativeArray();
                        UpdateVertices(vertices);

                        CmdBuffer.AddComponent<UpdateVertexColorTag>(child);
                    }

                    if (Children.Exists(child)) {
                        // Recurse into this
                        var childrenArray = Children[child].AsNativeArray();

                        UpdateAlphaOfChildren(childrenArray);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UpdateVertices(NativeArray<LocalVertexData> vertices) {
                for (int j = 0; j < vertices.Length; j++) {
                    var copy = vertices[j];
                    copy.Color = default;
                    vertices[j] = copy;
                }
            }
        }

        private EntityQuery disabledQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var localVertices = GetBufferFromEntity<LocalVertexData>();
            var children      = GetBufferFromEntity<Child>(true);
            var parents       = GetComponentDataFromEntity<Parent>(true);
            var cmdBuffer     = cmdBufferSystem.CreateCommandBuffer();

            var alphaJob      = new ZeroAlphaJob {
                CmdBuffer     = cmdBuffer,
                Children      = children,
                Parents       = parents,
                LocalVertices = localVertices
            };

            Dependency = Entities.ForEach((Entity entity) => {
                alphaJob.Execute(entity);
            }).WithAll<DisableRenderingTag, Disabled>().Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
