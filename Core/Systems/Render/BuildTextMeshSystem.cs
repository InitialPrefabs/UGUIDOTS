using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildTextMeshVertexSystem : JobComponentSystem {

        [BurstCompile]
        private struct BuildGlyphMapJob : IJobForEachWithEntity<FontID> {

            [WriteOnly]
            public NativeHashMap<int, Entity>.ParallelWriter GlyphMap;

            public void Execute(Entity entity, int index, ref FontID c0) {
                GlyphMap.TryAdd(c0.Value, entity);
            }
        }

        [BurstCompile]
        private struct BuildTextMeshJob : IJobChunk {

            [ReadOnly] public NativeHashMap<int, Entity> GlyphMap;
            [ReadOnly] public BufferFromEntity<GlyphElement> GlyphData;
            [ReadOnly] public ComponentDataFromEntity<FontFaceInfo> FontFaces;

            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkBufferType<CharElement> CharBufferType;
            [ReadOnly] public ArchetypeChunkComponentType<TextOptions> TextOptionType;

            public ArchetypeChunkBufferType<MeshVertexData> MeshVertexDataType;
            public ArchetypeChunkBufferType<TriangleIndexElement> TriangleIndexType;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var textBufferAccessor    = chunk.GetBufferAccessor(CharBufferType);
                var vertexBufferAccessor  = chunk.GetBufferAccessor(MeshVertexDataType);
                var triangleIndexAccessor = chunk.GetBufferAccessor(TriangleIndexType);
                var fontInfo              = chunk.GetNativeArray(TextOptionType);
                var entities              = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var text     = textBufferAccessor[i].AsNativeArray();
                    var vertices = vertexBufferAccessor[i];
                    var indices  = triangleIndexAccessor[i];
                    var fontID   = fontInfo[i].ID;

                    vertices.Clear();
                    indices.Clear();

                    var glyphTableExists = GlyphMap.TryGetValue(fontID, out var glyphEntity);
                    var glyphBufferExists = GlyphData.Exists(glyphEntity);

                    if (glyphTableExists && glyphBufferExists) {
                        var scale = (float)fontID / FontFaces[glyphEntity].DefaultFontSize;

                        var glyphData = GlyphData[glyphEntity].AsNativeArray();
                        TextMeshGenerationUtil.BuildTextMesh(ref vertices, ref indices, in text,
                            in glyphData, default, default, scale);
                    }

                    var textEntity = entities[i];
                    CmdBuffer.RemoveComponent<TextRebuildTag>(textEntity.Index, textEntity);
                }
            }
        }

        private EntityQuery glyphQuery, textQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            glyphQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<GlyphElement>(),
                    ComponentType.ReadOnly<FontID>()
                }
            });

            textQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadWrite<MeshVertexData>(),
                    ComponentType.ReadWrite<TriangleIndexElement>(),
                    ComponentType.ReadOnly<CharElement>(),
                    ComponentType.ReadOnly<TextOptions>(),
                    ComponentType.ReadOnly<TextRebuildTag>(),
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var glyphMap = new NativeHashMap<int, Entity>(glyphQuery.CalculateEntityCount(), Allocator.TempJob);

            var glyphMapDeps = new BuildGlyphMapJob {
                GlyphMap = glyphMap.AsParallelWriter()
            }.Schedule(glyphQuery, inputDeps);

            var textMeshDeps       = new BuildTextMeshJob {
                GlyphMap           = glyphMap,
                GlyphData          = GetBufferFromEntity<GlyphElement>(true),
                FontFaces          = GetComponentDataFromEntity<FontFaceInfo>(true),
                EntityType         = GetArchetypeChunkEntityType(),
                CharBufferType     = GetArchetypeChunkBufferType<CharElement>(),
                TextOptionType     = GetArchetypeChunkComponentType<TextOptions>(),
                MeshVertexDataType = GetArchetypeChunkBufferType<MeshVertexData>(),
                TriangleIndexType  = GetArchetypeChunkBufferType<TriangleIndexElement>(),
                CmdBuffer          = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(textQuery, glyphMapDeps);

            var finalDeps = glyphMap.Dispose(textMeshDeps);

            cmdBufferSystem.AddJobHandleForProducer(finalDeps);

            return finalDeps;
        }
    }
}
