using System.Runtime.CompilerServices;
using UGUIDots.Render;
using UGUIDots.Render.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    /// <summary>
    /// Forces an update of the children entities when the parent entity has a disabled tag.
    /// </summary>
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    [UpdateBefore(typeof(UpdateMeshSliceSystem))]
    public class ReactiveDisableHierarchySystem : SystemBase {

        private struct MarkVertexToUpdate {

            public EntityCommandBuffer CmdBuffer;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            public BufferFromEntity<LocalVertexData> LocalVertices;

            public void Execute(Entity entity) {
                CmdBuffer.RemoveComponent<DisableRenderingTag>(entity);
                if (Children.Exists(entity)) {
                    MarkChildWithTag(Children[entity].AsNativeArray());
                }
            }

            public void MarkChildWithTag(NativeArray<Child> children) {
                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    if (LocalVertices.Exists(child)) {
                        // var vertices = LocalVertices[child].AsNativeArray();
                        // UpdateVertices(vertices);

                        Debug.Log("Added to child");
                        CmdBuffer.AddComponent(child, new UpdateVertexColorTag { });
                    }

                    if (Children.Exists(child)) {
                        // Recurse into this
                        var childrenArray = Children[child].AsNativeArray();

                        MarkChildWithTag(childrenArray);
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

            var alphaJob      = new MarkVertexToUpdate {
                CmdBuffer     = cmdBuffer,
                Children      = children,
                Parents       = parents,
                LocalVertices = localVertices
            };

            Dependency = Entities.ForEach((Entity entity) => {
                alphaJob.Execute(entity);
            }).WithAll<NonInteractableTag, DisableRenderingTag>().Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
