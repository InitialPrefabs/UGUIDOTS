using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetAlignmentPosition(
            in GlyphElement glyph,
            in float lineHeight, 
            in TextAnchor anchor, 
            in Dimensions dimension) {

            switch (anchor) {
                case TextAnchor.UpperLeft:
                    throw new System.NotImplementedException();
                case TextAnchor.MiddleLeft:
                    throw new System.NotImplementedException();
                case TextAnchor.LowerLeft:
                    throw new System.NotImplementedException();
                case TextAnchor.UpperCenter:
                    return new float2(0, dimension.Value.y - lineHeight);
                case TextAnchor.MiddleCenter:
                    return new float2();
                case TextAnchor.LowerCenter:
                    return new float2(0, lineHeight);
                case TextAnchor.UpperRight:
                    throw new System.NotImplementedException();
                case TextAnchor.MiddleRight:
                    throw new System.NotImplementedException();
                case TextAnchor.LowerRight:
                    throw new System.NotImplementedException();
                default:
                    throw new System.ArgumentException("Invalid anchor, please use a valid TextAnchor");
            }
        }

        public static void BuildTextMesh(
            ref DynamicBuffer<MeshVertexData> vertices,
            ref DynamicBuffer<TriangleIndexElement> indices, 
            in NativeArray<CharElement> text,
            in NativeArray<GlyphElement> glyphs, 
            float2 startPos, 
            float scale, 
            FontStyle style, 
            float4 color,
            float spacing = 1f) {

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (glyphs.TryGetGlyph(in c, in style, out var glyph)) {
                    var baseIndex = (ushort)vertices.Length;

                    // Represents the bottom left hand corner
                    // Right now it is offsetted towards the upper right (looks like by half the height / width of the glyph?
                    var xPos = startPos.x + glyph.Bearings.x * scale;
                    var yPos = startPos.y - (glyph.Size.y - glyph.Bearings.y) * scale;

                    Debug.Log($"{xPos}, {yPos}");

                    Debug.Log(glyph.Bearings.x * scale);

                    var width  = glyph.Size.x * scale;
                    var height = glyph.Size.y * scale;
                    var right  = new float3(1, 0, 0);

                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos, yPos, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = glyph.UVBottomLeft()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos, yPos + height, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = glyph.UVTopLeft()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos + width, yPos + height, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = glyph.UVTopRight()
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos + width, yPos, 0),
                        Normal   = right,
                        Color    = color,
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
