using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UGUIDots.Transforms;
using TMPro;
using Unity.Collections;

namespace UGUIDots.Render {

    public static class TextUtil {

        public struct LineInfo {
            public float Width;
            public int Start, End;

            public override string ToString() {
                return $"Width: {Width}, Start Index: {Start}, End Index: {End}";
            }
        }

        // TODO: Calculate lines
        public static NativeList<LineInfo> GetLineInfo(
            in NativeArray<CharElement> text,
            in NativeArray<GlyphElement> glyphs,
            Dimensions dimensions,
            float fontScale,
            float2 stylePadding,
            out NativeHashMap<char, GlyphElement> glyphMappings) {

            glyphMappings = new NativeHashMap<char, GlyphElement>(text.Length, Allocator.Temp);
            var lineInfos = new NativeList<LineInfo>(Allocator.Temp);

            var lineWidth = 0f;
            var charCount = 0;
            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (!glyphs.TryGetGlyph(in c, out var glyph)) { continue; }
                glyphMappings.TryAdd(c, glyph);
            
                var xOffset    = (glyph.Bearings.x - stylePadding.x) * fontScale;
                var glyphWidth = glyph.Size.x + stylePadding.x * 2 * fontScale;
                var advance    = glyph.Advance * fontScale * stylePadding.y;

                if (lineWidth < dimensions.Width()) {
                    lineWidth += (xOffset + glyphWidth + advance);
                    charCount++;
                } else {
                    lineInfos.Add(new LineInfo {
                        Width  = lineWidth,
                        Start  = i - charCount,
                        End    = i
                    });

                    charCount = 0;
                    lineWidth = 0;
                }
            }
            return lineInfos;
        }

        /// <summary>
        /// Returns the relative vertical alignment of the text.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetVerticalAlignment(
            in FontFaceInfo fontFace,
            in TextOptions options,
            in float2 extents,
            float2 parentScale) {

            var fontScale = options.Size > 0 ? (float)options.Size / fontFace.PointSize : 1f;
            var alignment = options.Alignment;

            switch (alignment) {
                case var _ when (alignment & AnchoredState.TopRow) > 0: {
                        var ascentLine = fontFace.AscentLine * fontScale;
                        return extents.y - ascentLine;
                    }
                case var _ when (alignment & AnchoredState.MiddleRow) > 0: {
                        var avgLineHeight = (fontFace.LineHeight * fontScale) / 2 + (fontFace.DescentLine * fontScale);
                        return -avgLineHeight;
                    }
                case var _ when (alignment & AnchoredState.BottomRow) > 0: {
                        var descent = fontFace.DescentLine * fontScale;
                        return -extents.y - descent;
                    }
                default:
                    throw new System.ArgumentException("Invalid anchor, please use a valid TextAnchor");
            }
        }

        /// <summary>
        /// Determines the horizontal alignment of the text.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetHorizontalAlignment(
            in FontFaceInfo fontFace,
            in TextOptions options,
            in float2 extents) {

            var alignment = options.Alignment;

            switch (alignment) {
                case var _ when (alignment & AnchoredState.LeftColumn) > 0:
                    return -extents.x;
                default:
                    throw new System.ArgumentException("Invalid horizontal alignment, please use a valid TextAnchor!");
            }
        }

        /// <summary>
        /// Returns the associative padding between different styles.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SelectStylePadding(in TextOptions options, in FontFaceInfo faceInfo) {
            var isBold = options.Style == FontStyles.Bold;
            return 1.25f + math.select(faceInfo.NormalStyle.x, faceInfo.BoldStyle.x, isBold) / 4f;
        }
    }
}
