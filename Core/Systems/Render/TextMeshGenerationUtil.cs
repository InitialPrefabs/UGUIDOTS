using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using UGUIDots.Transforms;
using TMPro;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetVerticalAlignmentPosition(
            in FontFaceInfo fontFace,
            in TextOptions options,
            in Dimensions dimension,
            float2 parentScale)
        {

            var extents = (dimension.Value) / 2;
            var fontScale = options.Size > 0 ? (float)options.Size / fontFace.PointSize : 1f;
            var alignment = options.Alignment;

            switch (alignment)
            {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SelectStylePadding(in TextOptions options, in FontFaceInfo faceInfo) {
            var isBold = options.Style == FontStyles.Bold;
            return 1.25f + math.select(faceInfo.NormalStyle.x, faceInfo.BoldStyle.x, isBold) / 4f;
        }

        public static void NewBuildTextMesh(
            ref DynamicBuffer<MeshVertexData> vertices,
            ref DynamicBuffer<TriangleIndexElement> indices,
            in  NativeArray<CharElement> text,
            in  NativeArray<GlyphElement> glyphs,
            in  TextOptions options,
            in  AppliedColor color,
            in  FontFaceInfo faceInfo,
            in  float2 parentScale,
            ref float2 start) {

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (!glyphs.TryGetGlyph(in c, in options.Style, out var glyph)) {
                    continue;
                }

                var baseIndex = (ushort)vertices.Length;
                var fontScale = options.Size / (float)faceInfo.PointSize;

                var xPos = start.x + glyph.Advance * fontScale;
                var yPos = start.y - ((glyph.Size.y - glyph.Bearings.y) * fontScale);

                // TODO: Adjust the scale since we're doing ScaleWithScreenSize which is that new float2(1)
                var canvasScale  = options.Size * new float2(1) / faceInfo.PointSize;
                var stylePadding = SelectStylePadding(in options, in faceInfo);
                var uv1          = glyph.RawUV.NormalizeAdjustedUV(stylePadding, faceInfo.AtlasSize);
                // TODO: Figure out uv2

                var adjustedSize = glyph.Size * fontScale;
                var normal = new float3(new float3(1, 0, 0));

                vertices.Add(new MeshVertexData {
                    Position = new float3(xPos, yPos, 0),
                    Normal   = normal,
                    Color    = color.Value.ToNormalizedFloat4(),
                    UV       = uv1.c0
                });
                vertices.Add(new MeshVertexData {
                    Position = new float3(xPos, yPos + adjustedSize.y, 0),
                    Normal   = normal,
                    Color    = color.Value.ToNormalizedFloat4(),
                    UV       = uv1.c1
                });
                 vertices.Add(new MeshVertexData {
                    Position = new float3(xPos + adjustedSize.x, yPos + adjustedSize.y, 0),
                    Normal   = normal,
                    Color    = color.Value.ToNormalizedFloat4(),
                    UV       = uv1.c2
                });
                vertices.Add(new MeshVertexData {
                    Position = new float3(xPos + adjustedSize.x, yPos, 0),
                    Normal   = normal,
                    Color    = color.Value.ToNormalizedFloat4(),
                    UV       = uv1.c3
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

                start += new float2(glyph.Advance * fontScale, 0);
            }
        }

        public static void BuildTextMesh(
            ref        DynamicBuffer<MeshVertexData> vertices,
            ref        DynamicBuffer<TriangleIndexElement> indices,
            in         NativeArray<CharElement> text,
            in         NativeArray<GlyphElement> glyphs,
            float2     startPos,
            float      avgScale,
            FontStyles style,
            float4     color,
            float2     atlasSize,
            float      padding,
            float      fontScale,
            float      spacing = 1f) {

            UnityEngine.Debug.Log($"Mult: {fontScale * avgScale}");

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (glyphs.TryGetGlyph(in c, in style, out var glyph)) {
                    var baseIndex = (ushort)vertices.Length;

                    var xPos = startPos.x + glyph.Bearings.x * avgScale * fontScale;
                    var yPos = startPos.y - (glyph.Size.y - glyph.Bearings.y) * avgScale * fontScale;

                    var width = glyph.Size.x * avgScale * fontScale;
                    var height = glyph.Size.y * avgScale * fontScale;
                    var right = new float3(1, 0, 0);

                    var uv1 = glyph.RawUV.NormalizeAdjustedUV(padding, atlasSize);

                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos, yPos, 0),
                        Normal   = right,
                        Color    = color,
                        UV       = uv1.c0
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos, yPos + height, 0),
                        Normal   = right,
                        Color    = color,
                        UV       = uv1.c1
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos + width, yPos + height, 0),
                        Normal   = right,
                        Color    = color,
                        UV       = uv1.c2
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(xPos + width, yPos, 0),
                        Normal   = right,
                        Color    = color,
                        UV       = uv1.c3
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

                    startPos += new float2((glyph.Advance * spacing) * avgScale * fontScale, 0);
                }
            }
        }
    }
}
