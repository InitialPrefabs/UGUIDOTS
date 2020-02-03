using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UGUIDots.Transforms;
using TMPro;

namespace UGUIDots.Render {

    public static class TextUtil {

        // TODO: Calculate lines

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
