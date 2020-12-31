using UGUIDOTS.Collections;
using UGUIDOTS.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(BuildRenderHierarchySystem))]
    public unsafe class UpdateDynamicTextDataSystem : SystemBase {

        // NOTE: Must run off a single thread
        [BurstCompile]
        struct CollectDynamicTextJob : IJobChunk {

            public EntityCommandBuffer CommandBuffer;

            public NativeList<EntityPriority> PriorityQueue;

            [ReadOnly]
            public ComponentTypeHandle<StaticDataCount> StaticDataType;

            [ReadOnly]
            public EntityTypeHandle EntityTypeHandle;

            [ReadOnly]
            public BufferFromEntity<Child> ChildBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<DynamicTextTag> DynamicTexts;
            
            [ReadOnly]
            public ComponentDataFromEntity<SubmeshIndex> SubmeshIndices;

            [WriteOnly]
            public NativeHashMap<Entity, int2> StaticSpans;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var staticCount = chunk.GetNativeArray(StaticDataType);
                var entities = chunk.GetNativeArray(EntityTypeHandle);

                for (int i = 0; i < chunk.Count; i++) {
                    var staticSpan = staticCount[i];
                    var entity = entities[i];

                    var children = ChildBuffers[entity].AsNativeArray();

                    StaticSpans.Add(entity, staticSpan.AsInt2());

                    CommandBuffer.AddComponent<RebuildMeshTag>(entity);
                    CommandBuffer.RemoveComponent<OnDynamicTextChangeTag>(entity);
                    RecurseCollectDynamicText(children);
                }
            }

            void RecurseCollectDynamicText(NativeArray<Child> children) {
                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    if (DynamicTexts.HasComponent(child) && SubmeshIndices.HasComponent(child)) {
                        PriorityQueue.Add(new EntityPriority {
                            Entity       = child,
                            SubmeshIndex = SubmeshIndices[child].Value
                        });
                    }

                    if (ChildBuffers.HasComponent(child)) {
                        var grandChildren = ChildBuffers[child].AsNativeArray();

                        RecurseCollectDynamicText(grandChildren);
                    }
                }
            }
        }

        private EntityQuery canvasQuery;
        private EntityQuery dynamicTextQuery;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<ReferenceResolution>(), ComponentType.ReadOnly<Child>(),
                    ComponentType.ReadOnly<StaticDataCount>(), ComponentType.ReadOnly<OnDynamicTextChangeTag>()
                },
                None = new [] {
                    ComponentType.ReadOnly<OnResolutionChangeTag>()
                },
            });

            dynamicTextQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<CharElement>(), ComponentType.ReadOnly<DynamicTextTag>() }
            });

            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var children       = GetBufferFromEntity<Child>(true);
            var dynamicText    = GetComponentDataFromEntity<DynamicTextTag>(true);
            var submeshIndices = GetComponentDataFromEntity<SubmeshIndex>(true);
            var entityType     = GetEntityTypeHandle();

            var priorityQueue = new NativeList<EntityPriority>(dynamicTextQuery.CalculateEntityCount(), Allocator.TempJob);
            var staticSpanMap = new NativeHashMap<Entity, int2>(canvasQuery.CalculateEntityCount(), Allocator.TempJob);
            var submeshSliceMap = new NativeHashMap<int, Slice>(dynamicTextQuery.CalculateEntityCount(), Allocator.TempJob);

            var commandBuffer = commandBufferSystem.CreateCommandBuffer();

            Dependency           = new CollectDynamicTextJob {
                ChildBuffers     = children,
                DynamicTexts     = dynamicText,
                EntityTypeHandle = entityType,
                PriorityQueue    = priorityQueue,
                StaticDataType   = GetComponentTypeHandle<StaticDataCount>(),
                StaticSpans      = staticSpanMap,
                SubmeshIndices   = submeshIndices,
                CommandBuffer    = commandBuffer
            }.ScheduleSingle(canvasQuery, Dependency);

            Dependency          = new BuildDynamicTextJob {
                AppliedColors   = GetComponentDataFromEntity<AppliedColor>(true),
                CharBuffers     = GetBufferFromEntity<CharElement>(false),
                Dimensions      = GetComponentDataFromEntity<Dimension>(true),
                FontFaces       = GetComponentDataFromEntity<FontFaceInfo>(true),
                GlyphBuffers    = GetBufferFromEntity<GlyphElement>(true),
                Indices         = GetBufferFromEntity<Index>(false),
                Vertices        = GetBufferFromEntity<Vertex>(false),
                LinkedTextFont  = GetComponentDataFromEntity<LinkedTextFontEntity>(true),
                DynamicTexts   = priorityQueue,
                Roots           = GetComponentDataFromEntity<RootCanvasReference>(true),
                ScreenSpaces    = GetComponentDataFromEntity<ScreenSpace>(true),
                SubmeshIndices  = GetComponentDataFromEntity<SubmeshIndex>(true),
                StaticSpans     = staticSpanMap,
                SubmeshSliceMap = submeshSliceMap,
                TextOptions     = GetComponentDataFromEntity<TextOptions>(true),
                CommandBuffer   = commandBuffer
            }.Schedule(Dependency);

            var priorityQueueDisposal = priorityQueue.Dispose(Dependency);
            var staticSpanDisposal    = staticSpanMap.Dispose(Dependency);
            var submeshSliceDisposal  = submeshSliceMap.Dispose(Dependency);

            Dependency = JobHandle.CombineDependencies(priorityQueueDisposal, staticSpanDisposal, submeshSliceDisposal);
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
