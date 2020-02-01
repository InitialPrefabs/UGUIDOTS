using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UGUIDots.Transforms;
using TMPro;

namespace UGUIDots.Render {

    public static class TextMeshGenerationUtil {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetVerticalAlignmentPosition(
            in FontFaceInfo fontFace,
            in TextOptions options,
            in Dimensions dimension,
            float2 parentScale) {

            var extents   = (dimension.Value) / 2;
            var fontScale = options.Size > 0 ? (float)options.Size / fontFace.PointSize : 1f;
            var alignment = options.Alignment;

            switch (alignment) {
                case var _ when (alignment & AnchoredState.TopRow) > 0: {
                        var ascentLine = fontFace.AscentLine * fontScale;
                        var topLeft    = new float2(-extents.x, extents.y);
                        return topLeft - new float2(0, ascentLine);
                    }
                case var _ when (alignment & AnchoredState.MiddleRow) > 0: {
                        var avgLineHeight = (fontFace.LineHeight * fontScale) / 2 + (fontFace.DescentLine * fontScale);
                        return new float2(-extents.x, -avgLineHeight);
                    }
                case var _ when (alignment & AnchoredState.BottomRow) > 0: {
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
    }
}
