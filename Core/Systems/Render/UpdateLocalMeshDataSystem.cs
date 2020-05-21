using UGUIDots.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Render.Systems {
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class UpdateLocalMeshDataSystem : SystemBase {

        [BurstCompile]
        private struct UpdateLocalVertexJob : IJobChunk {

            public float3 Offset;

            [ReadOnly]
            public ArchetypeChunkComponentType<Disabled> DisabledType;

            [ReadOnly]
            public ArchetypeChunkComponentType<EnableRenderingTag> EnableRenderingType;

            [ReadOnly]
            public ArchetypeChunkComponentType<AppliedColor> AppliedColorType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [WriteOnly]
            public NativeHashMap<Entity, Entity>.ParallelWriter CanvasMap;

            public ArchetypeChunkBufferType<LocalVertexData> LocalVertexType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var colors         = chunk.GetNativeArray(AppliedColorType);
                var vertexBuffers  = chunk.GetBufferAccessor(LocalVertexType);
                var entities       = chunk.GetNativeArray(EntityType);
                var isDisabled     = chunk.Has(DisabledType);
                var isNewlyEnabled = chunk.Has(EnableRenderingType);

                // If newly disabled
                if (isDisabled && !isNewlyEnabled) {
                    Debug.Log($"Disabling by shoving with {Offset}");
                    for (int i = 0; i < chunk.Count; i++) {
                        var entity   = entities[i];
                        var vertices = vertexBuffers[i].AsNativeArray();
                        var color    = colors[i].Value.ToNormalizedFloat4();

                        for (int m = 0; m < vertices.Length; m++) {
                            var cpy       = vertices[m];
                            cpy.Color     = default;
                            cpy.Position += Offset;
                            vertices[m]   = cpy;
                        }

                        var root = HierarchyUtils.GetRoot(entity, Parents);
                        CanvasMap.TryAdd(root, entity);
                    }
                }

                // If this has been rendered to begin with
                if (!isDisabled && !isNewlyEnabled) {
                    for (int i       = 0; i < chunk.Count; i++) {
                        var entity   = entities[i];
                        var vertices = vertexBuffers[i].AsNativeArray();
                        var color    = colors[i].Value.ToNormalizedFloat4();

                        for (int m = 0; m < vertices.Length; m++) {
                            var cpy     = vertices[m];
                            cpy.Color   = color;
                            vertices[m] = cpy;
                        }

                        var root = HierarchyUtils.GetRoot(entity, Parents);
                        CanvasMap.TryAdd(root, entity);
                    }
                }

                // If the chunk is newly renabled
                if (!isDisabled && isNewlyEnabled) {
                    Debug.Log($"Enabled by -{Offset}");
                    for (int i = 0; i < chunk.Count; i++) {
                        var entity   = entities[i];
                        var vertices = vertexBuffers[i].AsNativeArray();
                        var color    = colors[i].Value.ToNormalizedFloat4();

                        for (int m = 0; m < vertices.Length; m++) {
                            var cpy       = vertices[m];
                            cpy.Color     = color;
                            cpy.Position -= Offset;
                            vertices[m]   = cpy;
                        }

                        var root = HierarchyUtils.GetRoot(entity, Parents);
                        CanvasMap.TryAdd(root, entity);
                    }
                }
            }
        }

        [BurstCompile]
        private struct ScheduleRootVertexUpdate : IJob {

            public EntityCommandBuffer CommandBuffer;
            public NativeHashMap<Entity, Entity> CanvasMap;

            public void Execute() {
                var keys = CanvasMap.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < keys.Length; i++) {
                    CommandBuffer.AddComponent<UpdateVertexColorTag>(keys[i]);
                }
                keys.Dispose();
            }
        }

        private EntityQuery canvasQuery, childrenUIQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {

            canvasQuery = GetEntityQuery(new EntityQueryDesc { 
                All = new [] { ComponentType.ReadOnly<WidthHeightRatio>() }
            });

            childrenUIQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadOnly<MeshDataSpan>(),
                    ComponentType.ReadWrite<LocalVertexData>(), ComponentType.ReadOnly<AppliedColor>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();

            RequireForUpdate(childrenUIQuery);
        }

        protected override void OnUpdate() {
            var map       = new NativeHashMap<Entity, Entity>(canvasQuery.CalculateEntityCount() * 2, Allocator.TempJob);
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            Dependency              = new UpdateLocalVertexJob {
                Offset              = new float3(Screen.width, Screen.height, 0) * 2,
                CanvasMap           = map.AsParallelWriter(),
                Parents             = GetComponentDataFromEntity<Parent>(),
                AppliedColorType    = GetArchetypeChunkComponentType<AppliedColor>(true),
                LocalVertexType     = GetArchetypeChunkBufferType<LocalVertexData>(false),
                EntityType          = GetArchetypeChunkEntityType(),
                EnableRenderingType = GetArchetypeChunkComponentType<EnableRenderingTag>(true),
                DisabledType        = GetArchetypeChunkComponentType<Disabled>(true)
            }.ScheduleParallel(childrenUIQuery, Dependency);

            Dependency        = new ScheduleRootVertexUpdate {
                CommandBuffer = cmdBuffer,
                CanvasMap     = map,
            }.Schedule(Dependency);

            Dependency = map.Dispose(Dependency);
            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
