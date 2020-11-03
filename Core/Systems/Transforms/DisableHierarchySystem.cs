using UGUIDOTS.Render;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS.Transforms.Systems {

    public class DisableHierarchySystem : SystemBase {

        [BurstCompile]
        struct HierarchyStateJob : IJobChunk {

            public EntityCommandBuffer CommandBuffer;

            // Entity Data to edit if they're enabled/disabled
            // --------------------------------------------------------------
            public ComponentDataFromEntity<Enabled> Enabled;

            [ReadOnly]
            public EntityTypeHandle EntityType;

            [ReadOnly]
            public ComponentTypeHandle<ToggleActiveStateTag> MarkStateType; // TODO: Remove this, but the query should have this

            [ReadOnly]
            public ComponentTypeHandle<Disabled> DisabledType;

            [ReadOnly]
            public ComponentDataFromEntity<MeshDataSpan> MeshDataSpans;

            [ReadOnly]
            public ComponentDataFromEntity<RootCanvasReference> RootCanvasReferences;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            // Spans to collect and enable/disable
            // --------------------------------------------------------------
            [WriteOnly]
            public NativeMultiHashMap<Entity, int2> ToDisable;

            [WriteOnly]
            public NativeMultiHashMap<Entity, int2> ToEnable;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                // Enable the entities and its children
                if (chunk.Has(DisabledType)) {
                    for (int i = 0; i < chunk.Count; i++) {
                        var entity = entities[i];

                        if (Enabled.HasComponent(entity)) {
                            Enabled[entity]       = new Enabled {
                                ActiveInHierarchy = true,
                                ActiveSelf        = true
                            };
                        }

                        if (RootCanvasReferences.HasComponent(entity) && MeshDataSpans.HasComponent(entity)) {
                            var root = RootCanvasReferences[entity].Value;
                            var span = MeshDataSpans[entity].VertexSpan;
                            ToDisable.Add(entity, span);
                        }

                        if (Children.HasComponent(entity)) {
                            var children = Children[entity].AsNativeArray();
                            RecurseChildren(children, true);
                        }

                        CommandBuffer.RemoveComponent<Disabled>(entity);
                    }
                } 
                // Otherwise disable the entity
                else {
                    for (int i = 0; i < chunk.Count; i++) {
                        var entity = entities[i];
                        if (Enabled.HasComponent(entity)) {
                            Enabled[entity]       = new Enabled {
                                ActiveInHierarchy = false,
                                ActiveSelf        = false
                            };
                        }

                        if (RootCanvasReferences.HasComponent(entity) && MeshDataSpans.HasComponent(entity)) {
                            var root = RootCanvasReferences[entity].Value;
                            var span = MeshDataSpans[entity].VertexSpan;
                            ToEnable.Add(entity, span);
                        }

                        if (Children.HasComponent(entity)) {
                            var children = Children[entity].AsNativeArray();
                            RecurseChildren(children, false);
                        }
                    }
                }
            }

            void RecurseChildren(NativeArray<Child> children, bool activeInHierarchy) {
                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    // TODO: Add the spans for the children.

                    if (activeInHierarchy) {
                        CommandBuffer.RemoveComponent<Disabled>(child);
                    } else {
                        CommandBuffer.AddComponent<Disabled>(child);
                    }

                    if (Enabled.HasComponent(child)) {
                        var enabled               = Enabled[child];
                        enabled.ActiveInHierarchy = activeInHierarchy;
                        Enabled[child]            = enabled;
                    }

                    if (Children.HasComponent(child)) {
                        var grandChildren = Children[child].AsNativeArray();

                        RecurseChildren(grandChildren, activeInHierarchy);
                    }
                }
            }
        }

        private EntityQuery enabledQuery;

        protected override void OnCreate() {
            enabledQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<Enabled>() },
                None = new [] { ComponentType.ReadOnly<Disabled>() }
            });
        }

        protected override void OnUpdate() {
            // TODO Implement
            var stateJob = new HierarchyStateJob {
            };
        }
    }
}
