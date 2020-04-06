using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UGUIDots.Transforms;
using TMPro;
using Unity.Collections;

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
                            lines.Add(new LineInfo {
                                LineWidth = diagnostics.LineWidth,
                                StartIndex = diagnostics.CharacterIndex
                            });

                            // Reset the character index to the current word we're scanning
                            diagnostics.CharacterIndex = i - diagnostics.WordCharCount + 1;
                            // Shift i to the current word because this word should go on the next line
                            i = i - diagnostics.WordCharCount + 1;

                            // We've effectively hit a new line and completed the previous word that 
                            // could fit on that line, also reset the word's width/line since we are scanning
                            // a new word on a new line.
                            diagnostics.WordCount     = 0;
                            diagnostics.WordCharCount = 0;
                            diagnostics.LineWidth     = diagnostics.WordWidth = 0f;

                        } else {
                            // We know that its potentially a giant blob of text so we need to 
                            // split the text - same way as how TMP does it
                            
                            lines.Add(new LineInfo {
                                LineWidth  = diagnostics.LineWidth - advance,
                                StartIndex = diagnostics.CharacterIndex
                            });

                            // Reset the widths and character counts of the words
                            diagnostics.LineCharCount = diagnostics.WordCharCount = 0;
                            diagnostics.LineWidth     = advance;
                            diagnostics.WordWidth     = 0f;

                            // Stoe the last known character index
                            diagnostics.CharacterIndex = i;
                        }
                    }
                    continue;
                }
                lines.Add(new LineInfo {
                    LineWidth  = diagnostics.LineWidth,
                    StartIndex = diagnostics.CharacterIndex
                });

                diagnostics.CharacterIndex = i;
                diagnostics.LineWidth      = diagnostics.WordWidth = 0;
                diagnostics.WordCount      = 0;
                diagnostics.WordCharCount  = 0;
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
        /// <param name="lineHeights">Stores the full, ascent, and descent line heights.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetVerticalAlignment(
            in float3 lineHeights,
            in float fontScale,
            in AnchoredState alignment,
            in float2 extents,
            in float textBlockHeight,
            in int lines) {

            switch (alignment) {
                case var _ when (alignment & AnchoredState.TopRow) > 0: {
                        var ascentLine = lineHeights.y * fontScale;
                        return extents.y - ascentLine;
                    }
                case var _ when (alignment & AnchoredState.MiddleRow) > 0: {
                        var avgLineHeight = (lineHeights.x * fontScale) * 0.5f + 
                            (lineHeights.z * fontScale) + (textBlockHeight * math.select(0f, 0.5f, lines > 1));
                        return -avgLineHeight;
                    }
                case var _ when (alignment & AnchoredState.BottomRow) > 0: {
                        var descent = lineHeights.z * fontScale;
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
            in AnchoredState alignment,
            in float2 extents,
            in float lineWidth) {

            switch (alignment) {
                case var _ when (alignment.IsAtColumn(AnchoredState.LeftColumn)):
                    return -extents.x;
                case var _ when (alignment.IsAtColumn(AnchoredState.CenterColumn)):
                    return -lineWidth / 2;
                case var _ when (alignment.IsAtColumn(AnchoredState.RightColumn)):
                    return extents.x - lineWidth;
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
            return 1.25f + (isBold ? faceInfo.BoldStyle.x / 4.0f : faceInfo.NormalStyle.x / 4.0f);
        }
    }
}
