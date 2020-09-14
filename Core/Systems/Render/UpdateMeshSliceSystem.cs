using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDOTS.Render.Systems {

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
            public EntityTypeHandle EntityType;

            [ReadOnly]
            public BufferTypeHandle<RenderElement> RenderBufferType;

            [ReadOnly]
            public ComponentTypeHandle<UpdateVertexColorTag> UpdateVertexType;

            [ReadOnly]
            public ComponentTypeHandle<DisableRenderingTag> DisableRenderingType;

            public EntityCommandBuffer.ParallelWriter CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);
                var renderBuffers = chunk.GetBufferAccessor(RenderBufferType);

                if (chunk.Has(DisableRenderingType)) {
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

                        CmdBuffer.RemoveComponent<DisableRenderingTag>(chunkIndex, entity);
                        CmdBuffer.AddComponent<BatchCanvasTag>(chunkIndex, entity);
                    }
                }

                if (chunk.Has(UpdateVertexType)) {
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
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ConsolidateRenderElements(in NativeArray<RenderElement> renderBatches, 
                ref NativeList<RootVertexData> rootVertices) {
                for (int i = 0; i < renderBatches.Length; i++) {
                    var entity = renderBatches[i].Value;

                    if (UpdateVerticesData.HasComponent(entity)) {
                        CmdBuffer.RemoveComponent<UpdateVertexColorTag>(entity.Index, entity);
                    }

                    if (LocalVertexDatas.HasComponent(entity)) {
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
                    ComponentType.ReadWrite<RootVertexData>(), ComponentType.ReadOnly<RenderElement>()
                },
                Any = new[] {
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadOnly<DisableRenderingTag>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            Dependency               = new UpdateMeshSliceJob {
                ChildrenBuffer       = GetBufferFromEntity<Child>(true),
                LocalVertexDatas     = GetBufferFromEntity<LocalVertexData>(true),
                UpdateVerticesData   = GetComponentDataFromEntity<UpdateVertexColorTag>(true),
                EntityType           = GetEntityTypeHandle(),
                RenderBufferType     = GetBufferTypeHandle<RenderElement>(true),
                UpdateVertexType     = GetComponentTypeHandle<UpdateVertexColorTag>(true),
                DisableRenderingType = GetComponentTypeHandle<DisableRenderingTag>(true),
                CmdBuffer            = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            }.Schedule(canvasUpdateQuery, Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
