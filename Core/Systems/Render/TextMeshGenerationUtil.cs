using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        public static void BuildTextMesh(ref DynamicBuffer<MeshVertexData> vertices,
            ref DynamicBuffer<TriangleIndexElement> indices, in NativeArray<TextElement> text,
            in NativeArray<GlyphElement> glyphs, float2 startPox, float2 maxDimensions) {

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (glyphs.GetGlyphOf(in c, out var glyph)) {

                }
            }

            throw new System.NotImplementedException();
        }
    }
}
