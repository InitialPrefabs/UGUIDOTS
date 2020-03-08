using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class UpdateLocalMeshDataSystem : SystemBase {

        [RequireComponentTag(typeof(UpdateVertexColorTag))]
        [BurstCompile]
        private struct UpdateLocalVertexJob : IJobForEachWithEntity_EBCC<LocalVertexData, MeshDataSpan, AppliedColor> {

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [WriteOnly]
            public NativeHashMap<Entity, Entity>.ParallelWriter CanvasMap;

            public void Execute(Entity entity, int index, DynamicBuffer<LocalVertexData> b0, ref MeshDataSpan c1, 
                ref AppliedColor c2) {

                var vertices = b0.AsNativeArray();

                for (int i      = 0; i < vertices.Length; i++) {
                    var cpy     = vertices[i];
                    cpy.Color   = c2.Value.ToNormalizedFloat4();
                    vertices[i] = cpy;
                }

                var root = GetRoot(in Parents, entity);
                CanvasMap.TryAdd(root, entity);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Entity GetRoot(in ComponentDataFromEntity<Parent> parents, in Entity entity) {
                if (!parents.Exists(entity)) {
                    return entity;
                }

                return GetRoot(in parents, parents[entity].Value);
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

        private EntityQuery cachedMeshQuery, canvasQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cachedMeshQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadOnly<CachedMeshTag>(), 
                    ComponentType.ReadWrite<LocalVertexData>(),
                }
            });

            canvasQuery = GetEntityQuery(new EntityQueryDesc { 
                All = new [] { ComponentType.ReadOnly<WidthHeightRatio>() }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();

            RequireForUpdate(cachedMeshQuery);
        }

        protected override void OnUpdate() {
            var map = new NativeHashMap<Entity, Entity>(canvasQuery.CalculateEntityCount() * 2, Allocator.TempJob);
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            Dependency        = new UpdateLocalVertexJob {
                Parents       = GetComponentDataFromEntity<Parent>(),
                CanvasMap     = map.AsParallelWriter(),
            }.Schedule(this, Dependency);


            Dependency        = new ScheduleRootVertexUpdate {
                CommandBuffer = cmdBuffer,
                CanvasMap     = map
            }.Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
            Dependency = map.Dispose(Dependency);
        }
    }
}
