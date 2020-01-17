using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildTextMeshVertexSystem : JobComponentSystem {

        private struct BuildTextMeshJob : IJobChunk {

            [ReadOnly] public NativeArray<GlyphElement> GlyphData;
            [ReadOnly] public ArchetypeChunkBufferType<CharElement> TextBufferType;
            [ReadOnly] public ArchetypeChunkComponentType<TextOptions> FontSizeType;

            public ArchetypeChunkBufferType<MeshVertexData> MeshVertexDataType;
            public ArchetypeChunkBufferType<TriangleIndexElement> TriangleIndexType;
            public float DefaultFontSize;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var textBufferAccessor    = chunk.GetBufferAccessor(TextBufferType);
                var vertexBufferAccessor  = chunk.GetBufferAccessor(MeshVertexDataType);
                var triangleIndexAccessor = chunk.GetBufferAccessor(TriangleIndexType);
                var fontSizes             = chunk.GetNativeArray(FontSizeType);

                for (int i = 0; i < chunk.Count; i++) {
                    var text     = textBufferAccessor[i].AsNativeArray();
                    var vertices = vertexBufferAccessor[i];
                    var indices  = triangleIndexAccessor[i];
                    var fontSize = fontSizes[i].Size;

                    vertices.Clear();
                    indices.Clear();

                    var scale = fontSize / DefaultFontSize;

                    TextMeshGenerationUtil.BuildTextMesh(ref vertices, ref indices, in text, 
                        in GlyphData, default, default, scale);
                }
            }
        }

        private EntityQuery glyphQuery, textQuery;

        protected override void OnCreate() {
            glyphQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<GlyphElement>()
                }
            });

            textQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadWrite<MeshVertexData>(),
                    ComponentType.ReadWrite<TriangleIndexElement>(),
                    ComponentType.ReadOnly<CharElement>(),
                    ComponentType.ReadOnly<TextOptions>(),
                    ComponentType.ReadOnly<TextRebuildTag>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
