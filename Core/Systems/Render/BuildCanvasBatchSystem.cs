using UGUIDots.Collections.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup))]
    public class BuildCanvasBatchSystem : JobComponentSystem {

        private struct BuildBatchSystem : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<MeshVertexData> MeshVertexData;

            [ReadOnly]
            public BufferFromEntity<TriangleIndexElement> Triangles;

            [ReadOnly]
            public ArchetypeChunkComponentType<MeshBatches> MeshBatchType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var batches        = chunk.GetNativeArray(MeshBatchType);
                var canvasEntities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var canvas   = canvasEntities[i];
                    var batch    = batches[i];
                    var entities = batch.Elements;
                    var spans    = batch.Spans;

                    var vertices = new NativeList<MeshVertexData>(Allocator.Temp);
                    var indices = new NativeList<TriangleIndexElement>(Allocator.Temp);

                    // Fill up the vertice and indices
                    LoopMeshBatch(ref entities, ref spans, ref vertices, ref indices);

                    CommandBuffer.AddComponent<BatchedCanvasRenderingData>(canvas.Index, canvas);
                    CommandBuffer.SetComponent(canvas.Index, canvas, new BatchedCanvasRenderingData {
                        Vertices = UnsafeArray<MeshVertexData>.FromNativeList(ref vertices, Allocator.Persistent),
                        Indices  = UnsafeArray<TriangleIndexElement>.FromNativeList(ref indices, 
                            Allocator.Persistent)
                    });

                    vertices.Dispose();
                    indices.Dispose();
                }
            }

            unsafe void LoopMeshBatch(ref UnsafeArray<Entity> entities, ref UnsafeArray<int2> spans, 
                ref NativeList<MeshVertexData> vertices, ref NativeList<TriangleIndexElement> indices) {
                for (int i = 0; i < spans.Length; i++) {
                    // NOTE: When we first start then we have an offset of 0. As we continue to loop through
                    // the indices, we increment the indexCount and multiply by 4;
                    var indexCount = 0;

                    for (int start = spans[i].x; start < spans[i].y; start++, indexCount++) {
                        var currentEntity = entities[start];

                        if (!MeshVertexData.Exists(currentEntity) || !Triangles.Exists(currentEntity)) {
                            continue;
                        }

                        var vertexStart = vertices.Length;
                        var localVertices = MeshVertexData[currentEntity].AsNativeArray();
                        vertices.AddRange(localVertices);

                        // NOTE: Inclusive span of the start and end
                        var vertexSpan = new int2(vertexStart, vertices.Length - 1);

                        var indexStart = indices.Length;
                        var localIndices = Triangles[currentEntity].AsNativeArray();
                        for (int l = 0; l < localIndices.Length; l++) {
                            var newIndex = (ushort)(localIndices[i].Value + (indexCount * 4));
                            indices.Add(newIndex);
                        }
                        var indexSpan = new int2(indexStart, indices.Length);

                        // Add the spans so we know which meshes are sliced to which sections
                        CommandBuffer.AddComponent(currentEntity.Index, currentEntity, new MeshSpan {
                            VertexSpan = vertexSpan,
                            IndexSpan  = indexSpan
                        });

                        // NOTE: Multiply the indexCount by 4 so we can add it to the original indices.
                        indexCount++;
                    }
                }
            }
        }

        private EntityQuery unbatchedCanvasGroup;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            unbatchedCanvasGroup = GetEntityQuery(new EntityQueryDesc {
                All  = new [] { ComponentType.ReadOnly<MeshBatches>() },
                None = new [] { ComponentType.ReadWrite<BatchedCanvasRenderingData>() }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var vertices = GetBufferFromEntity<MeshVertexData>(true);
            var indices  = GetBufferFromEntity<TriangleIndexElement>(true);

            var meshBatchType = GetArchetypeChunkComponentType<MeshBatches>(true);
            var entityType    = GetArchetypeChunkEntityType();

            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            var deps = new BuildBatchSystem {
                MeshVertexData = vertices,
                Triangles      = indices,
                MeshBatchType  = meshBatchType,
                EntityType     = entityType,
                CommandBuffer  = cmdBuffer
            }.Schedule(unbatchedCanvasGroup, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(deps);
            return deps;
        }
    }
}
