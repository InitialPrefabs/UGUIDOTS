using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace UGUIDots.Transforms.Systems {

    /// <summary>
    /// Forces an update of the children entities when the parent entity has a disabled tag.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ReactiveDisableHierarchySystem : JobComponentSystem {

        private struct DisableChildrenJob : IJobChunk {

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public BufferFromEntity<Child> ChildBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<Disabled> Disableds;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var current = entities[i];
                    RecurseDisable(in current);
                }
            }

            private void RecurseDisable(in Entity parent) {

                if (!ChildBuffers.Exists(parent)) {
                    return;
                }

                var children = ChildBuffers[parent];

                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;
                    if (!Disableds.Exists(child)) {
                        CmdBuffer.AddComponent<Disabled>(child.Index, child);
                    }
                    RecurseDisable(in child);
                }
            }
        }

        private EntityQuery disabledParentsQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            disabledParentsQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] { ComponentType.ReadOnly<Disabled>(), ComponentType.ReadOnly<Child>() }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var disabledDeps = new DisableChildrenJob {
                EntityType = GetArchetypeChunkEntityType(),
                Disableds = GetComponentDataFromEntity<Disabled>(true),
                CmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(disabledParentsQuery, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(disabledDeps);
            return disabledDeps;
        }
    }
}
