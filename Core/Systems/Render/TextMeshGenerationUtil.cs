using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UGUIDots.Transforms;
using TMPro;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetVerticalAlignmentPosition(
            in FontFaceInfo fontFace,
            in TextOptions options,
            in Dimensions dimension) {

            var extents    = (dimension.Value) / 2;
            var fontScale  = options.Size > 0 ? (float)options.Size / fontFace.DefaultFontSize : 1f;
            var alignment  = options.Alignment;

            switch (alignment) {
                case var _ when (alignment & AnchoredState.TopRow) > 0:
                {
                    var ascentLine = fontFace.AscentLine * fontScale;
                    var topLeft    = new float2(-extents.x, extents.y);
                    return topLeft - new float2(0, ascentLine);
                }
                case var _ when (alignment & AnchoredState.MiddleRow) > 0:
                {
                    var avgLineHeight = (fontFace.LineHeight * fontScale) / 2 + (fontFace.DescentLine * fontScale);
                    return new float2(-extents.x, -avgLineHeight);
                }
                case var _ when (alignment & AnchoredState.BottomRow) > 0:
                {
                    throw new System.NotImplementedException();
                }
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
            FontStyles style, 
            float4 color,
            float2 atlasSize,
            float padding,
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

                    var uvs = glyph.RawUV.NormalizeAdjustedUV(padding, atlasSize);

                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos, yPos, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = uvs.c0
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos, yPos + height, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = uvs.c1
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos + width, yPos + height, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = uvs.c2
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos + width, yPos, 0),
                        Normal   = right,
                        Color    = color,
                        UVs      = uvs.c3
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
