using System.Runtime.CompilerServices;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms {

    /// <summary>
    /// Flags which state how a transform is anchored relative to the resolution of the screen.
    /// </summary>
    public enum AnchoredState : int {
        BottomLeft   = 0b1001,
        BottomCenter = 0b1010,
        BottomRight  = 0b1100,
        MiddleLeft   = 0b10010,
        MiddleCenter = 0b10100,
        MiddleRight  = 0b11000,
        TopLeft      = 0b100100,
        TopCenter    = 0b101000,
        TopRight     = 0b110000,

        BottomRow    = 0b1000,
        MiddleRow    = 0b10000,
        TopRow       = 0b100000,
        LeftColumn   = 0b0001,
        CenterColumn = 0b0010,
        RightColumn  = 0b0100
    }

    /// <summary>
    /// Attempts to mimic Unity's anchoring data. This stores an unscaled distance to an anchor 
    /// relative to its parent's position.
    /// </summary>
    public struct Anchor : IComponentData {
        public float2 Offset;
        public AnchoredState State;
    }

    public static class AnchorExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ShiftRightBy(this AnchoredState state, int shift) {
            var value = (int)state;
            return value >> shift;
        }

        /// <summary>
        /// Transforms a rect transform's anchor preset to an enum.
        /// </summary>
        public static AnchoredState ToAnchor(this RectTransform transform) {
            var min = transform.anchorMin;

            AnchoredState anchor = default;

            { // Bottom Row
                if (min == default) {
                    anchor = AnchoredState.BottomLeft;
                }

                if (min == new Vector2(0.5f, 0f)) {
                    anchor = AnchoredState.BottomCenter;
                }

                if (min == new Vector2(1f, 0f)) {
                    anchor = AnchoredState.BottomRight;
                }
            }

            { // Middle Row
                if (min == new Vector2(0f, 0.5f)) {
                    anchor = AnchoredState.MiddleLeft;
                }

                if (min == new Vector2(0.5f, 0.5f)) {
                    anchor = AnchoredState.MiddleCenter;
                }

                if (min == new Vector2(1f, 0.5f)) {
                    anchor = AnchoredState.MiddleRight;
                }
            }

            { // Top Row
                if (min == new Vector2(0f, 1f)) {
                    anchor = AnchoredState.TopLeft;
                }

                if (min == new Vector2(0.5f, 1f)) {
                    anchor = AnchoredState.TopCenter;
                }

                if (min == new Vector2(1f, 1f)) {
                    anchor = AnchoredState.TopRight;
                }
            }

            return anchor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 RelativeAnchorTo(this in Anchor anchor, int2 resolution, float2 scale) {
            switch (anchor.State) {
                case AnchoredState.BottomLeft:
                    return float2.zero + anchor.Offset * scale;
                case AnchoredState.MiddleLeft:
                    return new float2(0, resolution.y / 2) + anchor.Offset * scale;
                case AnchoredState.TopLeft:
                    return new float2(0, resolution.y) + anchor.Offset * scale;
                case AnchoredState.BottomCenter:
                    return new float2(resolution.x / 2, 0) + anchor.Offset * scale;
                case AnchoredState.MiddleCenter:
                    return new float2(resolution / 2) + anchor.Offset * scale;
                case AnchoredState.TopCenter:
                    return new float2(resolution.x / 2, resolution.y) + anchor.Offset * scale;
                case AnchoredState.BottomRight:
                    return new float2(resolution.x, 0) + anchor.Offset * scale;
                case AnchoredState.MiddleRight:
                    return new float2(resolution.x, resolution.y / 2) + anchor.Offset * scale;
                case AnchoredState.TopRight:
                    return resolution + anchor.Offset;
                default:
                    throw new System.ArgumentException("Anchored State is invalid!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 RelativeAnchorTo(this in Anchor anchor, int2 size, float2 scale, float2 center) {
            var relativeAnchorOffset = anchor.RelativeAnchorTo(size, scale);
            return relativeAnchorOffset + center;
        }

        /// <summary>
        /// Switches TextAnchor to their AnchoredState equivalent.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnchoredState FromTextAnchor(this TextAlignmentOptions anchor) {
            switch (anchor) {
                case TextAlignmentOptions.BottomLeft:
                    return AnchoredState.BottomLeft;
                case TextAlignmentOptions.Bottom:
                    return AnchoredState.BottomCenter;
                case TextAlignmentOptions.BottomRight:
                    return AnchoredState.BottomRight;
                case TextAlignmentOptions.Left:
                    return AnchoredState.MiddleLeft;
                case TextAlignmentOptions.Center:
                    return AnchoredState.MiddleCenter;
                case TextAlignmentOptions.Right:
                    return AnchoredState.MiddleRight;
                case TextAlignmentOptions.TopLeft:
                    return AnchoredState.TopLeft;
                case TextAlignmentOptions.Top:
                    return AnchoredState.TopCenter;
                case TextAlignmentOptions.TopRight:
                    return AnchoredState.TopRight;
                default:
                    throw new System.ArgumentException($"Cannot convert {anchor} to a valid AnchoredState!");
            }
        }

        /// <summary>
        /// Performs bitwise operation to check if an AnchorState is on a specific row.
        /// </summary>
        /// <param name="lhs">The left hand parameter to compare with</param>
        /// <param name="rhs">The right hand parameter to compare to</param>
        /// <returns>True, if they are on the same row<returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAtRow(this AnchoredState lhs, AnchoredState rhs) {
            return (lhs & rhs) > 0;
        }

        /// <summary>
        /// Performs bit shifting and bitwise operation to check that an anchor is on the same column.
        /// </summary>
        /// <param name="column">The column to check against</param>
        /// <param name="state">The column to check with</param>
        /// <returns>True, if the column matches</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAtColumn(this AnchoredState state, AnchoredState column) {
            switch (state) {
                case var _ when IsAtRow(state, AnchoredState.TopRow):
                    return (state.ShiftRightBy(2) & (int)column) > 0;
                case var _ when IsAtRow(state, AnchoredState.MiddleRow):
                    return (state.ShiftRightBy(1) & (int)column) > 0;
                case var _ when IsAtRow(state, AnchoredState.BottomRow):
                    return (state & column) > 0;
                default:
                    return false;
            }
        }
    }
}
