using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        public static void BuildTextMesh(ref DynamicBuffer<MeshVertexData> vertices,
            ref DynamicBuffer<TriangleIndexElement> indices, in NativeArray<CharElement> text,
            in NativeArray<GlyphElement> glyphs, float2 startPos, float2 maxDimensions, float scale) {

            var baseIndex = (ushort)vertices.Length;

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (glyphs.GetGlyphOf(in c, out var glyph)) {
                    
                    var xPos = startPos.x + glyph.Bearings.x * scale;
                    var yPos = startPos.y - (glyph.Size.y - glyph.Bearings.y) * scale;

                    var width  = new half(glyph.Size.x * scale);
                    var height = new half(glyph.Size.y * scale);
                    var right  = new half3(new float3(1, 0, 0));

                    vertices.Add(new MeshVertexData {
                        Position = new half3((half)xPos, (half)yPos, default),
                        Normal   = right,
                        UVs      = glyph.UVBottomLeftAsHalf2()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new half3((half)xPos, (half)(yPos + height), default),
                        Normal   = right,
                        UVs      = glyph.UVTopLeftAsHalf2()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new half3(new half(xPos + width), new half(yPos + height), default),
                        Normal   = right,
                        UVs      = glyph.UVTopRightAsHalf2()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new half3(new half(xPos + width), new half(yPos), default),
                        Normal   = right,
                        UVs      = glyph.UVBottomRightAsHalf2()
                    });

                    var bl = baseIndex;
                    var tl = (ushort)(baseIndex + 1);
                    var tr = (ushort)(baseIndex + 2);
                    var br = (ushort)(baseIndex + 3);

                    indices.Add(new TriangleIndexElement { Value = bl });
                    indices.Add(new TriangleIndexElement { Value = tl });
                    indices.Add(new TriangleIndexElement { Value = tr });

                    indices.Add(new TriangleIndexElement { Value = bl });
                    indices.Add(new TriangleIndexElement { Value = tr });
                    indices.Add(new TriangleIndexElement { Value = br });
                }
            }
        }
    }
}
