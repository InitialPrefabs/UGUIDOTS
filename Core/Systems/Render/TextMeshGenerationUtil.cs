using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UGUIDots.Transforms;
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace UGUIDots.Render {

    public static class TextUtil {

        struct LineDiagnostics {
            public float LineWidth;
            public float WordWidth;
            public int LineCharCount;
            public int WordCharCount;
            public int WordCount;
            public int CharacterIndex;
        }

        public struct LineInfo {
            public float LineWidth;
            public int StartIndex;

            public override string ToString() {
                return $"LineWidth: {LineWidth}, StartIndex: {StartIndex}";
            }
        }

        public static void CountLines(
            in NativeArray<CharElement> text,
            in NativeArray<GlyphElement> glyphs,
            Dimensions dimensions,
            float padding,
            ref NativeList<LineInfo> lines) {

            var diagnostics = new LineDiagnostics { };

            for (int i = 0; i < text.Length; i++) {
                var c = text[i].Value;

                if (!glyphs.TryGetGlyph(in c, out var glyph)) {
                    continue;
                }

                // TODO: Add support for \n
                if (c == ' ') {
                    diagnostics.WordWidth      = 0;
                    diagnostics.WordCharCount  = -1;

                    // We hit a space so there's a new word coming up supposedly
                    diagnostics.WordCount++;
                }

                var advance = glyph.Advance * padding;

                if (diagnostics.LineWidth < dimensions.Width()) {
                    diagnostics.WordCharCount++;
                    diagnostics.LineCharCount++;
                    diagnostics.LineWidth += advance;
                    diagnostics.WordWidth += advance;

                    if (diagnostics.LineWidth > dimensions.Width()) {
                        if (diagnostics.WordCount != 0) {
                            // We know we have multiple words already...
                            // TODO: Figure this out
                        } else {
                            // We know that its potentially a giant blob of text so we need to 
                            // split the text - same way as how TMP does it
                            
                            lines.Add(new LineInfo {
                                LineWidth  = diagnostics.LineWidth,
                                StartIndex = diagnostics.CharacterIndex
                            });

                            // Reset the widths and character counts of the words
                            diagnostics.LineCharCount = diagnostics.WordCharCount = 0;
                            diagnostics.LineWidth     = diagnostics.WordWidth = 0f;

                            // Stoe the last known character index
                            diagnostics.CharacterIndex = i;
                        }
                    }
                    continue;
                }
            }

            // Add the last line
            lines.Add(new LineInfo  {
                LineWidth  = diagnostics.LineWidth,
                StartIndex = diagnostics.CharacterIndex
            });
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
