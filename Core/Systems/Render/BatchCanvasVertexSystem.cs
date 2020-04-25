using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup))]
    public class BatchCanvasVertexSystem : SystemBase {

        [BurstCompile]
        private struct BatchSubMeshJob : IJobChunk {

            [ReadOnly]
            public ComponentDataFromEntity<LinkedMaterialEntity> MaterialKeys;

            [ReadOnly]
            public ComponentDataFromEntity<LinkedTextureEntity> TextureKeys;

            [ReadOnly]
            public ArchetypeChunkBufferType<RenderElement> RenderType;

            [ReadOnly]
            public ArchetypeChunkBufferType<BatchedSpanElement> SpanType;

            public ArchetypeChunkBufferType<SubmeshKeyElement> SubMeshType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var renders      = chunk.GetBufferAccessor(RenderType);
                var batchedSpans = chunk.GetBufferAccessor(SpanType);
                var submeshes    = chunk.GetBufferAccessor(SubMeshType);

                for (int i = 0; i < chunk.Count; i++) {
                    var batchedSubMesh = submeshes[i];
                    var renderer       = renders[i].AsNativeArray();
                    var span           = batchedSpans[i].AsNativeArray();

                    batchedSubMesh.Clear();

                    for (int k = 0; k < span.Length; k++) {
                        var current = span[k].Value;

                        // When we build the keys - we only care about the first rendered element's material
                        // and texture, because we assume via batching that the first element's material and
                        // and texture are representative of the entire batch.
                        var element = renderer[current.x].Value;

                        var materialKey = MaterialKeys.Exists(element) ? MaterialKeys[element].Value : Entity.Null;
                        var textureKey  = TextureKeys.Exists(element) ? TextureKeys[element].Value : Entity.Null;

                        batchedSubMesh.Add(new SubmeshKeyElement {
                            TextureEntity  = textureKey,
                            MaterialEntity = materialKey
                        });
                    }
                }
            }
        }

        [BurstCompile]
        private struct BatchCanvasJob : IJobChunk {

            public ArchetypeChunkBufferType<RootVertexData>           CanvasVertexType;
            public ArchetypeChunkBufferType<RootTriangleIndexElement> CanvasIndexType;
            public ArchetypeChunkBufferType<SubmeshSliceElement>      SubMeshType;

            [ReadOnly]
            public BufferFromEntity<LocalVertexData> MeshVertices;

            [ReadOnly]
            public BufferFromEntity<LocalTriangleIndexElement> TriangleIndices;

            [ReadOnly]
            public ArchetypeChunkBufferType<RenderElement> RenderElementType;

            [ReadOnly]
            public ArchetypeChunkBufferType<BatchedSpanElement> SpanType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities         = chunk.GetNativeArray(EntityType);
                var vertexAccessor   = chunk.GetBufferAccessor(CanvasVertexType);
                var triangleAccessor = chunk.GetBufferAccessor(CanvasIndexType);
                var batchedRenders   = chunk.GetBufferAccessor(RenderElementType);
                var batchedSpans     = chunk.GetBufferAccessor(SpanType);
                var submeshDescriptors        = chunk.GetBufferAccessor(SubMeshType);

                for (int i             = 0; i < chunk.Count; i++) {
                    var entity         = entities[i];
                    var rootVertices   = vertexAccessor[i];
                    var rootTriangles  = triangleAccessor[i];
                    var renderElements = batchedRenders[i].AsNativeArray();
                    var spans          = batchedSpans[i].AsNativeArray();
                    var submesh        = submeshDescriptors[i];

                    rootVertices.Clear();
                    rootTriangles.Clear();
                    submesh.Clear();

                    PopulateRootCanvas(in renderElements, in spans, ref rootVertices, ref rootTriangles, ref submesh);

                    // The canvas does not have to be rebatched.
                    CommandBuffer.RemoveComponent<BatchCanvasTag>(entity.Index, entity);
                    CommandBuffer.AddComponent<BuildCanvasTag>(entity.Index, entity);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void PopulateRootCanvas(in NativeArray<RenderElement> renders, in NativeArray<BatchedSpanElement> spans,
                ref DynamicBuffer<RootVertexData> rootVertices, ref DynamicBuffer<RootTriangleIndexElement> rootIndices,
                ref DynamicBuffer<SubmeshSliceElement> submeshes) {

                int entityCount = 0;
                for (int j = 0; j < spans.Length; j++) {
                    var currentSpan        = spans[j].Value;
                    var subMeshVertexStart = rootVertices.Length;
                    var subMeshIndexStart  = rootIndices.Length;

                    for (int k = currentSpan.x; k < currentSpan.y + currentSpan.x; k++, entityCount++) {
                        var childEntity   = renders[k].Value;
                        var childVertices = MeshVertices[childEntity].AsNativeArray().Reinterpret<RootVertexData>();
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
                    submeshes.Add(new SubmeshSliceElement {
                        VertexSpan = new int2(subMeshVertexStart, rootVertices.Length - subMeshVertexStart),
                        IndexSpan  = new int2(subMeshIndexStart, rootIndices.Length - subMeshIndexStart)
                    });

                }
            }

            // TODO: Support 9 slicing because the offset only assumes you have 6 vertices per image
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void AddAdjustedIndex(int offset, ref DynamicBuffer<RootTriangleIndexElement> indices,
                in NativeArray<LocalTriangleIndexElement> localTriangles) {

                var nextStartIndex = indices.Length > 1 ? indices[indices.Length - 1] + 1 : 0;

                for (int x = 0; x < localTriangles.Length; x++) {
                    indices.Add((ushort)(localTriangles[x].Value + nextStartIndex));
                }
            }
        }

        private EntityQuery unbatchedCanvasGroup;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            unbatchedCanvasGroup = GetEntityQuery(new EntityQueryDesc {
                All  = new [] {
                    ComponentType.ReadOnly<RenderElement>(), ComponentType.ReadWrite<SubmeshKeyElement>(),
                    ComponentType.ReadWrite<RootVertexData>(), ComponentType.ReadWrite<RootTriangleIndexElement>(),
                    ComponentType.ReadOnly<BatchCanvasTag>(), ComponentType.ReadOnly<BatchedSpanElement>(),
                    ComponentType.ReadOnly<Child>(),
                },
            });
        }

        protected override void OnUpdate() {
            var renderType   = GetArchetypeChunkBufferType<RenderElement>(true);
            var spanType     = GetArchetypeChunkBufferType<BatchedSpanElement>(true);
            Dependency       = new BatchSubMeshJob {
                MaterialKeys = GetComponentDataFromEntity<LinkedMaterialEntity>(true),
                TextureKeys  = GetComponentDataFromEntity<LinkedTextureEntity>(true),
                RenderType   = renderType,
                SpanType     = spanType,
                SubMeshType  = GetArchetypeChunkBufferType<SubmeshKeyElement>()
            }.Schedule(unbatchedCanvasGroup, Dependency);

            Dependency            = new BatchCanvasJob {
                CanvasVertexType  = GetArchetypeChunkBufferType<RootVertexData>(),
                CanvasIndexType   = GetArchetypeChunkBufferType<RootTriangleIndexElement>(),
                SubMeshType       = GetArchetypeChunkBufferType<SubmeshSliceElement>(),
                MeshVertices      = GetBufferFromEntity<LocalVertexData>(true),
                TriangleIndices   = GetBufferFromEntity<LocalTriangleIndexElement>(true),
                RenderElementType = renderType,
                SpanType          = spanType,
                EntityType        = GetArchetypeChunkEntityType(),
                CommandBuffer     = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(unbatchedCanvasGroup, Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
