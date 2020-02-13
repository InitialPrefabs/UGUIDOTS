using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup))]
    public class BuildCanvasVertexBatchSystem : JobComponentSystem {

        private struct BuildSubmeshBufferJob : IJobForEach_BB<SubmeshMaterialIdxElement, Child> {

            [ReadOnly]
            public ComponentDataFromEntity<MaterialKey> MaterialKeys;

            public void Execute(DynamicBuffer<SubmeshMaterialIdxElement> b0, DynamicBuffer<Child> b1) {
                b0.Clear();

                for (int i = 0; i < b1.Length; i++) {
                    var child = b1[i].Value;

                    if (!MaterialKeys.Exists(child)) {
                        continue;
                    }

                    var id = MaterialKeys[child].Value;
                    b0.Add(id);
                }
            }
        }

        private struct BuildCanvasJob : IJobChunk {

            public ArchetypeChunkBufferType<CanvasVertexData> CanvasVertexType;
            public ArchetypeChunkBufferType<CanvasIndexElement> CanvasIndexType;

            [ReadOnly]
            public BufferFromEntity<VertexData> MeshVertices;

            [ReadOnly]
            public BufferFromEntity<TriangleIndexElement> TriangleIndices;

            [ReadOnly]
            public ArchetypeChunkBufferType<RenderElement> RenderElementType;

            [ReadOnly]
            public ArchetypeChunkBufferType<BatchedSpanElement> SpanType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;


            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                // TODO: Loop through the RenderEntities and fill the vertices
                // TODO: Add the data spans to the images and texts.
                var entities         = chunk.GetNativeArray(EntityType);
                var vertexAccessor   = chunk.GetBufferAccessor(CanvasVertexType);
                var triangleAccessor = chunk.GetBufferAccessor(CanvasIndexType);
                var batchedRenders   = chunk.GetBufferAccessor(RenderElementType);
                var batchedSpans     = chunk.GetBufferAccessor(SpanType);

                for (int i             = 0; i < chunk.Count; i++) {
                    var entity         = entities[i];
                    var rootVertices   = vertexAccessor[i];
                    var rootTriangles  = triangleAccessor[i];
                    var renderElements = batchedRenders[i].AsNativeArray();
                    var spans          = batchedSpans[i].AsNativeArray();

                    rootVertices.Clear();
                    rootTriangles.Clear();

                    PopulateRootCanvas(in renderElements, in spans, ref rootVertices, ref rootTriangles);

                    // The canvas is no longer dirty
                    CommandBuffer.RemoveComponent<DirtyTag>(entity.Index, entity);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void PopulateRootCanvas(in NativeArray<RenderElement> renders, in NativeArray<BatchedSpanElement> spans,
                ref DynamicBuffer<CanvasVertexData> rootVertices, ref DynamicBuffer<CanvasIndexElement> rootIndices) {

                int entityCount = 0;
                for (int j = 0; j < spans.Length; j++) {
                    var currentSpan = spans[j].Value;

                    for (int k = currentSpan.x; k < currentSpan.y; k++, entityCount++) {
                        var childEntity   = renders[k].Value;
                        var childVertices = MeshVertices[childEntity].AsNativeArray().Reinterpret<CanvasVertexData>();
                        var childIndices  = TriangleIndices[childEntity].AsNativeArray();

                        var startVertexIndex   = rootVertices.Length;
                        var startTriangleIndex = rootIndices.Length;

                        rootVertices.AddRange(childVertices);
                        AddAdjustedIndex(entityCount, ref rootIndices, in childIndices);

                        CommandBuffer.AddComponent<MeshDataSpan>(childEntity.Index, childEntity);
                        CommandBuffer.SetComponent(childEntity.Index, childEntity, new MeshDataSpan {
                            VertexSpan = new int2(startVertexIndex, childVertices.Length),
                            IndexSpan  = new int2(startTriangleIndex, childIndices.Length)
                        });
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void AddAdjustedIndex(int offset, ref DynamicBuffer<CanvasIndexElement> indices, 
                in NativeArray<TriangleIndexElement> localTriangles) {

                for (int x = 0; x < localTriangles.Length; x++) {
                    indices.Add((ushort)(localTriangles[x].Value + (offset * 4)));
                }
            }
        }

        private EntityQuery unbatchedCanvasGroup;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            unbatchedCanvasGroup = GetEntityQuery(new EntityQueryDesc {
                All  = new [] { 
                    ComponentType.ReadOnly<RenderElement>(), ComponentType.ReadWrite<SubmeshMaterialIdxElement>(),
                    ComponentType.ReadWrite<CanvasVertexData>(), ComponentType.ReadWrite<CanvasIndexElement>(),
                    ComponentType.ReadOnly<DirtyTag>(), ComponentType.ReadOnly<BatchedSpanElement>(),
                    ComponentType.ReadOnly<Child>(),
                },
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var submeshMatJob = new BuildSubmeshBufferJob {
                MaterialKeys = GetComponentDataFromEntity<MaterialKey>(true)
            }.Schedule(unbatchedCanvasGroup, inputDeps);

            var batchDeps         = new BuildCanvasJob {
                MeshVertices      = GetBufferFromEntity<VertexData>(true),
                TriangleIndices   = GetBufferFromEntity<TriangleIndexElement>(true),
                CanvasVertexType  = GetArchetypeChunkBufferType<CanvasVertexData>(),
                CanvasIndexType   = GetArchetypeChunkBufferType<CanvasIndexElement>(),
                RenderElementType = GetArchetypeChunkBufferType<RenderElement>(true),
                SpanType          = GetArchetypeChunkBufferType<BatchedSpanElement>(true),
                EntityType        = GetArchetypeChunkEntityType(),
                CommandBuffer     = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(unbatchedCanvasGroup, inputDeps);

            var finalDeps = JobHandle.CombineDependencies(submeshMatJob, batchDeps);
            cmdBufferSystem.AddJobHandleForProducer(finalDeps);

            return finalDeps;
        }
    }
}
