using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

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
}
