using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup))]
    public class BatchCanvasVertexSystem : JobComponentSystem {

        [BurstCompile]
        private struct BuildSubmeshBufferJob : IJobForEach_BBB<RenderElement, BatchedSpanElement, SubMeshKeyElement> { 

            [ReadOnly]
            public ComponentDataFromEntity<MaterialKey> MaterialKeys;

            [ReadOnly]
            public ComponentDataFromEntity<TextureKey> TextureKeys;

            public void Execute(DynamicBuffer<RenderElement> b0, DynamicBuffer<BatchedSpanElement> b1, DynamicBuffer<SubMeshKeyElement> b2) {
                b2.Clear();
                for (int i = 0; i < b1.Length; i++) {
                    var current = b1[i].Value;
                    var element = b0[current.x].Value;

                    short materialKey = (short)math.select(-1, MaterialKeys[element].Value, MaterialKeys.Exists(element));
                    short textureKey  = (short)math.select(-1, TextureKeys[element].Value, TextureKeys.Exists(element));

                    b2.Add(new SubMeshKeyElement {
                        TextureKey  = textureKey,
                        MaterialKey = materialKey
                    });
                }
            }
        }

        [BurstCompile]
        private struct BuildCanvasJob : IJobChunk {

            public ArchetypeChunkBufferType<CanvasVertexData>    CanvasVertexType;
            public ArchetypeChunkBufferType<CanvasIndexElement>  CanvasIndexType;
            public ArchetypeChunkBufferType<SubMeshSliceElement> SubMeshType;

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
                var submeshes        = chunk.GetBufferAccessor(SubMeshType);

                for (int i             = 0; i < chunk.Count; i++) {
                    var entity         = entities[i];
                    var rootVertices   = vertexAccessor[i];
                    var rootTriangles  = triangleAccessor[i];
                    var renderElements = batchedRenders[i].AsNativeArray();
                    var spans          = batchedSpans[i].AsNativeArray();
                    var submesh = submeshes[i];

                    rootVertices.Clear();
                    rootTriangles.Clear();

                    PopulateRootCanvas(in renderElements, in spans, ref rootVertices, ref rootTriangles, ref submesh);

                    // The canvas is no longer dirty
                    CommandBuffer.RemoveComponent<DirtyTag>(entity.Index, entity);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void PopulateRootCanvas(in NativeArray<RenderElement> renders, in NativeArray<BatchedSpanElement> spans,
                ref DynamicBuffer<CanvasVertexData> rootVertices, ref DynamicBuffer<CanvasIndexElement> rootIndices,
                ref DynamicBuffer<SubMeshSliceElement> submeshes) {

                int entityCount = 0;
                for (int j = 0; j < spans.Length; j++) {
                    var currentSpan  = spans[j].Value;
                    var subMeshVertexStart = rootVertices.Length;
                    var subMeshIndexStart = rootIndices.Length;

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

                    submeshes.Add(new SubMeshSliceElement {
                        VertexSpan = new int2(subMeshVertexStart, rootVertices.Length),
                        IndexSpan  = new int2(subMeshIndexStart, rootVertices.Length)
                    });
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
                    ComponentType.ReadOnly<RenderElement>(), ComponentType.ReadWrite<SubMeshKeyElement>(),
                    ComponentType.ReadWrite<CanvasVertexData>(), ComponentType.ReadWrite<CanvasIndexElement>(),
                    ComponentType.ReadOnly<DirtyTag>(), ComponentType.ReadOnly<BatchedSpanElement>(),
                    ComponentType.ReadOnly<Child>(),
                },
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var submeshMatJob = new BuildSubmeshBufferJob {
                MaterialKeys = GetComponentDataFromEntity<MaterialKey>(true),
                TextureKeys  = GetComponentDataFromEntity<TextureKey>()
            }.Schedule(unbatchedCanvasGroup, inputDeps);

            var batchDeps         = new BuildCanvasJob {
                CanvasVertexType  = GetArchetypeChunkBufferType<CanvasVertexData>(),
                CanvasIndexType   = GetArchetypeChunkBufferType<CanvasIndexElement>(),
                SubMeshType       = GetArchetypeChunkBufferType<SubMeshSliceElement>(),
                MeshVertices      = GetBufferFromEntity<VertexData>(true),
                TriangleIndices   = GetBufferFromEntity<TriangleIndexElement>(true),
                RenderElementType = GetArchetypeChunkBufferType<RenderElement>(true),
                SpanType          = GetArchetypeChunkBufferType<BatchedSpanElement>(true),
                EntityType        = GetArchetypeChunkEntityType(),
                CommandBuffer     = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(unbatchedCanvasGroup, submeshMatJob);

            var finalDeps = JobHandle.CombineDependencies(submeshMatJob, batchDeps);
            cmdBufferSystem.AddJobHandleForProducer(finalDeps);

            return finalDeps;
        }
    }
}
