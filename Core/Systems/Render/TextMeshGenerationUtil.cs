using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        public static void BuildTextMesh(ref DynamicBuffer<MeshVertexData> vertices,
            ref DynamicBuffer<TriangleIndexElement> indices, in NativeArray<CharElement> text,
            in NativeArray<GlyphElement> glyphs, float2 startPos, float2 maxDimensions, float scale, 
            float spacing = 1f) {

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (glyphs.GetGlyphOf(in c, out var glyph)) {
                    var baseIndex = (ushort)vertices.Length;
                   
                    var xPos = startPos.x + glyph.Bearings.x * scale;
                    var yPos = startPos.y - (glyph.Size.y - glyph.Bearings.y) * scale;

                    var width  = glyph.Size.x * scale;
                    var height = glyph.Size.y * scale;
                    var right  = new float3(1, 0, 0);

                    vertices.Add(new MeshVertexData {
                        Position = new float2(xPos, yPos),
                        Normal   = right,
                        UVs      = glyph.UVBottomLeft()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float2(xPos, yPos + height),
                        Normal   = right,
                        UVs      = glyph.UVTopLeft()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float2(xPos + width, yPos + height),
                        Normal   = right,
                        UVs      = glyph.UVTopRight()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float2(xPos + width, yPos),
                        Normal   = right,
                        UVs      = glyph.UVBottomRight()
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

                    startPos += new float2((glyph.Advance * spacing) * scale, 0);
                }
            }
        }
    }
}
