using System.Runtime.CompilerServices;
using UGUIDots.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class UpdateLocalMeshDataSystem : SystemBase {

        [BurstCompile]
        private struct UpdateLocalVertexJob : IJobChunk {

            [ReadOnly]
            public ComponentDataFromEntity<Disabled> Disabled;

            [ReadOnly]
            public ArchetypeChunkComponentType<AppliedColor> AppliedColorType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            public ArchetypeChunkBufferType<LocalVertexData> LocalVertexType;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [WriteOnly]
            public NativeHashMap<Entity, Entity>.ParallelWriter CanvasMap;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var colors        = chunk.GetNativeArray(AppliedColorType);
                var vertexBuffers = chunk.GetBufferAccessor(LocalVertexType);
                var entities      = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity   = entities[i];
                    if (!Disabled.Exists(entity)) {

                        var color    = colors[i];
                        var vertices = vertexBuffers[i].AsNativeArray();

                        for (int k = 0; k < vertices.Length; k++) {
                            var cpy     = vertices[k];
                            cpy.Color   = color.Value.ToNormalizedFloat4();
                            vertices[k] = cpy;
                        }
                    }

                    var root = HierarchyUtils.GetRoot(entity, Parents);
                    CanvasMap.TryAdd(root, entity);
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
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();

            RequireForUpdate(childrenUIQuery);
        }

        protected override void OnUpdate() {
            var map = new NativeHashMap<Entity, Entity>(canvasQuery.CalculateEntityCount() * 2, Allocator.TempJob);
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            Dependency           = new UpdateLocalVertexJob {
                Parents          = GetComponentDataFromEntity<Parent>(),
                CanvasMap        = map.AsParallelWriter(),
                AppliedColorType = GetArchetypeChunkComponentType<AppliedColor>(true),
                LocalVertexType  = GetArchetypeChunkBufferType<LocalVertexData>(false),
                EntityType       = GetArchetypeChunkEntityType(),
                Disabled         = GetComponentDataFromEntity<Disabled>(false)
            }.Schedule(childrenUIQuery, Dependency);

            Dependency        = new ScheduleRootVertexUpdate {
                CommandBuffer = cmdBuffer,
                CanvasMap     = map,
            }.Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
            Dependency = map.Dispose(Dependency);
        }
    }
}
