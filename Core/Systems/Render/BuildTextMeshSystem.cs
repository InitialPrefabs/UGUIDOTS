using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildTextMeshVertexSystem : JobComponentSystem {

        private struct BuildTextMeshJob : IJobChunk {

            public NativeArray<GlyphElement> GlyphData;
            public ArchetypeChunkBufferType<TextElement> TextBufferType;
            public ArchetypeChunkBufferType<MeshVertexData> MeshVertexDataType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var textBufferAccessor = chunk.GetBufferAccessor(TextBufferType);
                var vertexBufferAccesor = chunk.GetBufferAccessor(MeshVertexDataType);

                for (int i = 0; i < chunk.Count; i++) {
                    var textBuffer = textBufferAccessor[i];
                    var vertexBuffer = vertexBufferAccesor[i];

                    vertexBuffer.Clear();
                }
            }

            private void BuildTextMeshData(in DynamicBuffer<TextElement> txt, 
                ref DynamicBuffer<MeshVertexData> vertices) {

                vertices.Clear();

                for (int i = 0; i < txt.Length; i++) {
                    var c = txt[i];

                    // TODO: Support Dynamic Fonts
                    GlyphData.GetGlyphOf(c, out GlyphElement glyph);

                    // TODO: Get the glyph
                    // TODO: Get the start position of the text in local position of the dimensions
                    // TODO: Add word wrapping if word wrapping is enabled
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
