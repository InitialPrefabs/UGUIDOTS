using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetAlignmentPosition(
            in FontFaceInfo fontFace,
            in TextOptions options,
            in float2 canvasScale,
            in Dimensions dimension) {

            var extents   = (dimension.Value) / 2;
            var fontScale = (float)options.Size / fontFace.DefaultFontSize;

            Debug.Log($"Scale: {canvasScale}, Font Scale: {fontScale}");

            switch (options.Alignment) {
                case TextAnchor.UpperLeft:
                {
                    var topLeft = new float2(-extents.x, extents.y);
                    var ascentLine = fontFace.AscentLine * fontScale;
                    return topLeft - new float2(0, ascentLine);
                }
                case TextAnchor.MiddleLeft:
                {
                }
                    throw new System.NotImplementedException();
                case TextAnchor.LowerLeft:
                    throw new System.NotImplementedException();
                case TextAnchor.UpperCenter:
                    throw new System.NotImplementedException();
                case TextAnchor.MiddleCenter:
                    throw new System.NotImplementedException();
                case TextAnchor.LowerCenter:
                    throw new System.NotImplementedException();
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

                    var xPos = startPos.x + glyph.Bearings.x * scale;
                    var yPos = startPos.y - (glyph.Size.y - glyph.Bearings.y) * scale;

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
