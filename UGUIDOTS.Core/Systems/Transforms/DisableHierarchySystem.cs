using System.Runtime.CompilerServices;
using UGUIDOTS.Render;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDOTS.Transforms.Systems {

    // TODO: Eventually make this into a multithreaded system.
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
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
                            ToEnable.Add(root, span);
                        }

                        if (Children.HasComponent(entity)) {
                            var children = Children[entity].AsNativeArray();
                            RecurseChildren(children, true);
                        }

                        CommandBuffer.RemoveComponent<Disabled>(entity);
                        CommandBuffer.RemoveComponent<ToggleActiveStateTag>(entity);
                    }
                } 
                // Otherwise disable the entities and its children
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
                            ToDisable.Add(root, span);
                        }

                        if (Children.HasComponent(entity)) {
                            var children = Children[entity].AsNativeArray();
                            RecurseChildren(children, false);
                        }

                        CommandBuffer.AddComponent<Disabled>(entity);
                        CommandBuffer.RemoveComponent<ToggleActiveStateTag>(entity);
                    }
                }
            }

            void RecurseChildren(NativeArray<Child> children, bool activeInHierarchy) {
                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    // TODO: Add the spans for the children to edit in another job.


                    if (RootCanvasReferences.HasComponent(child) && MeshDataSpans.HasComponent(child)) {
                        var root = RootCanvasReferences[child].Value;
                        var span = MeshDataSpans[child].VertexSpan;

                        if (activeInHierarchy) {
                            CommandBuffer.RemoveComponent<Disabled>(child);
                            ToEnable.Add(root, span);
                        } else {
                            CommandBuffer.AddComponent<Disabled>(child);
                            ToDisable.Add(root, span);
                        }
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

        [BurstCompile]
        struct ShiftVerticesOnStateJob : IJob {

            public EntityCommandBuffer CommandBuffer;

            public BufferFromEntity<Vertex> Vertices;

            [ReadOnly]
            public NativeMultiHashMap<Entity, int2> ToEnable;

            [ReadOnly]
            public NativeMultiHashMap<Entity, int2> ToDisable;

            public void Execute() {
                var offset = new float3(OffsetConstants.DisabledOffset, 0);
                ShiftVertices(ToEnable, offset);
                ShiftVertices(ToDisable, -offset);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe void ShiftVertices(NativeMultiHashMap<Entity, int2> map, float3 offset) {
                var keys = map.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < keys.Length; i++) {
                    var rootEntity = keys[i];
                    var vertices = Vertices[rootEntity].AsNativeArray();
                    Vertex* start = (Vertex*)vertices.GetUnsafePtr();

                    map.TryGetFirstValue(rootEntity, out int2 span, out NativeMultiHashMapIterator<Entity> it);
                    for (int j = 0; j < span.y; j++) {
                        Vertex* ptr = start + j + span.x;
                        ptr->Position += offset;
                    }

                    while (map.TryGetNextValue(out span, ref it)) {
                        for (int j = 0; j < span.y; j++) {
                            Vertex* ptr = start + j + span.x;
                            ptr->Position += offset;
                        }
                    }

                    CommandBuffer.AddComponent<RebuildMeshTag>(rootEntity);
                }
            }
        }

        private EntityCommandBufferSystem entityCommandBufferSystem;
        private EntityQuery toEnableQuery;
        private EntityQuery enableQuery;

        protected override void OnCreate() {
            toEnableQuery = GetEntityQuery(new EntityQueryDesc {
                All  = new [] { 
                    ComponentType.ReadOnly<Enabled>(), ComponentType.ReadOnly<ToggleActiveStateTag>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });

            enableQuery = GetEntityQuery(new EntityQueryDesc {
                All  = new [] { 
                    ComponentType.ReadOnly<Enabled>(), ComponentType.ReadOnly<MeshDataSpan>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });

            entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            RequireForUpdate(toEnableQuery);
        }

        protected unsafe override void OnUpdate() {
            var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

            var size = enableQuery.CalculateEntityCount();
            var toEnable  = new NativeMultiHashMap<Entity, int2>(size, Allocator.TempJob);
            var toDisable = new NativeMultiHashMap<Entity, int2>(size, Allocator.TempJob);

            var stateJob             = new HierarchyStateJob {
                Children             = GetBufferFromEntity<Child>(false),
                DisabledType         = GetComponentTypeHandle<Disabled>(true),
                EntityType           = GetEntityTypeHandle(),
                MeshDataSpans        = GetComponentDataFromEntity<MeshDataSpan>(true),
                Enabled              = GetComponentDataFromEntity<Enabled>(false),
                RootCanvasReferences = GetComponentDataFromEntity<RootCanvasReference>(true),
                ToEnable             = toEnable,
                ToDisable            = toDisable,
                CommandBuffer        = commandBuffer
            };

            stateJob.Run(toEnableQuery);

            var vertexBuffers = GetBufferFromEntity<Vertex>(false);

            // Jobs to shift the vertices based on what container they're in
            // --------------------------------------------------------------
            var shiftJob = new ShiftVerticesOnStateJob {
                CommandBuffer = commandBuffer,
                ToDisable     = toDisable,
                ToEnable      = toEnable,
                Vertices      = vertexBuffers
            };

            shiftJob.Run();

            toEnable.Dispose();
            toDisable.Dispose();

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
