using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class UpdateMeshSliceSystem : SystemBase {

        [RequireComponentTag(typeof(UpdateVertexColorTag))]
        [BurstCompile]
        private struct UpdateMeshSliceJob : IJobForEachWithEntity_EB<RootVertexData> {

            [ReadOnly]
            public BufferFromEntity<Child> ChildrenBuffer;

            [ReadOnly]
            public ComponentDataFromEntity<MeshDataSpan> MeshDataSpans;

            [ReadOnly]
            public BufferFromEntity<LocalVertexData> LocalVertexDatas;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(Entity entity, int index, DynamicBuffer<RootVertexData> b0) {
                if (!ChildrenBuffer.Exists(entity)) {
                    return;
                }

                var rootVertices = b0.AsNativeArray();
                RecurseUpdateChildren(entity, ref rootVertices);

                CmdBuffer.RemoveComponent<UpdateVertexColorTag>(index, entity);
                CmdBuffer.AddComponent<BuildCanvasTag>(index, entity);
            }

            private void RecurseUpdateChildren(Entity root, ref NativeArray<RootVertexData> rootVertices) {
                if (!ChildrenBuffer.Exists(root)) {
                    return;
                }

                var children = ChildrenBuffer[root].AsNativeArray();
                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    if (!MeshDataSpans.Exists(child) || !LocalVertexDatas.Exists(child)) {
                        continue;
                    }

                    var span = MeshDataSpans[child];
                    var localVertices = LocalVertexDatas[child].AsNativeArray();

                    for (int k = 0; k < localVertices.Length; k++) {
                        rootVertices[span.VertexSpan.x + k] = RootVertexData.FromLocalVertexData(localVertices[k]);
                    }

                    CmdBuffer.RemoveComponent<UpdateVertexColorTag>(child.Index, child);

                    RecurseUpdateChildren(child, ref rootVertices);
                }
            }
        }

        private EntityQuery canvasUpdateQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            canvasUpdateQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadWrite<RootVertexData>(), 
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            Dependency           = new UpdateMeshSliceJob {
                ChildrenBuffer   = GetBufferFromEntity<Child>(true),
                MeshDataSpans    = GetComponentDataFromEntity<MeshDataSpan>(true),
                LocalVertexDatas = GetBufferFromEntity<LocalVertexData>(true),
                CmdBuffer        = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(canvasUpdateQuery, Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }

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

    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class ButtonColorStateSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery buttonColorQuery;

        protected override void OnCreate() {
            buttonColorQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<AppliedColor>() , ComponentType.ReadOnly<ColorStates>()
                },
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            Dependency = Entities.WithStoreEntityQueryInField(ref buttonColorQuery).
                ForEach((Entity entity, in AppliedColor c0, in ColorStates c1, in CursorState c2) => {

                bool delta = false;
                Color32 color = default;

                // TODO: Redo how button clicks are registered.
                switch (c2.State) {
                    case var _ when c2.State == ButtonState.Hover && 
                        !c1.HighlightedColor.ToFloat4().Equals(c0.Value.ToFloat4()):
                        color = c1.HighlightedColor;
                        delta = true;
                        break;

                    case var _ when c2.State == ButtonState.Pressed &&
                        !c1.PressedColor.ToFloat4().Equals(c0.Value.ToFloat4()):
                        color = c1.PressedColor;
                        delta = true;
                        break;

                        /*
                    case var _ when c2.State == ButtonState.Hover && c2.Held &&
                        !c1.SelectedColor.ToFloat4().Equals(c0.Value.ToFloat4()):
                        color = c1.SelectedColor;
                        delta = true;
                        break;
                        */

                    case var _ when c2.State == ButtonState.None &&
                        !c1.DefaultColor.ToFloat4().Equals(c0.Value.ToFloat4()):
                        color = c1.DefaultColor;
                        delta = true;
                        break;
                }

                if (delta) {
                    cmdBuffer.SetComponent(entity.Index, entity, new AppliedColor { Value = color });
                    cmdBuffer.AddComponent<UpdateVertexColorTag>(entity.Index, entity);
                }
            }).WithBurst().ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
