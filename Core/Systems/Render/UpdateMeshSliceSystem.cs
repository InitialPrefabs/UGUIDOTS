using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class UpdateMeshSliceSystem : SystemBase {

        [BurstCompile]
        private unsafe struct UpdateMeshSliceJob : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<Child> ChildrenBuffer;

            [ReadOnly]
            public BufferFromEntity<LocalVertexData> LocalVertexDatas;

            [ReadOnly]
            public ComponentDataFromEntity<UpdateVertexColorTag> UpdateVerticesData;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public ArchetypeChunkBufferType<RenderElement> RenderBufferType;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);
                var renderBuffers = chunk.GetBufferAccessor(RenderBufferType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity         = entities[i];
                    var renderElements = renderBuffers[i].AsNativeArray();
                    var rootVertices   = new NativeList<RootVertexData>(Allocator.Temp);
                    ConsolidateRenderElements(renderElements, ref rootVertices);

                    var rootBuffer = CmdBuffer.SetBuffer<RootVertexData>(chunkIndex, entity);
                    rootBuffer.ResizeUninitialized(rootVertices.Length);

                    UnsafeUtility.MemCpy(rootBuffer.GetUnsafePtr(), rootVertices.GetUnsafePtr(),
                        UnsafeUtility.SizeOf<RootVertexData>() * rootVertices.Length);

                    rootVertices.Dispose();

                    CmdBuffer.RemoveComponent<UpdateVertexColorTag>(chunkIndex, entity);
                    CmdBuffer.AddComponent<BuildCanvasTag>(chunkIndex, entity);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ConsolidateRenderElements(in NativeArray<RenderElement> renderBatches, 
                ref NativeList<RootVertexData> rootVertices) {
                for (int i = 0; i < renderBatches.Length; i++) {
                    var entity = renderBatches[i].Value;

                    if (UpdateVerticesData.Exists(entity)) {
                        CmdBuffer.RemoveComponent<UpdateVertexColorTag>(entity.Index, entity);
                    }

                    if (LocalVertexDatas.Exists(entity)) {
                        var localVertices = LocalVertexDatas[entity].AsNativeArray();
                        for (int m = 0; m < localVertices.Length; m++) {
                            rootVertices.Add(localVertices[m]);
                        }
                    }
                }
            }
        }

        private EntityQuery canvasUpdateQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            canvasUpdateQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadWrite<RootVertexData>(),
                    ComponentType.ReadOnly<RenderElement>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            Dependency             = new UpdateMeshSliceJob {
                ChildrenBuffer     = GetBufferFromEntity<Child>(true),
                LocalVertexDatas   = GetBufferFromEntity<LocalVertexData>(true),
                UpdateVerticesData = GetComponentDataFromEntity<UpdateVertexColorTag>(true),
                EntityType         = GetArchetypeChunkEntityType(),
                RenderBufferType   = GetArchetypeChunkBufferType<RenderElement>(true),
                CmdBuffer          = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(canvasUpdateQuery, Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
