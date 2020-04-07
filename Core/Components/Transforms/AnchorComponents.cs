using System.Runtime.CompilerServices;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Transforms {

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
    /// relative to its aprents and the anchor state.
    /// </summary>
    public struct Anchor : IComponentData {
        public AnchoredState State;
        public float2 Distance;
    }

    public static class AnchorExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ShiftRightBy(this AnchoredState state, int shift) {
            var value = (int)state;
            return value >> shift;
        }

        /// <summary>
        /// Returns the position that the anchor is supposedly anchored to.
        /// </summary>
        /// <param name="state">The current state of the anchor.</param>
        /// <param name="res">The current resolution we want to consider.</param>
        /// <returns>The relative screenspace position that the anchor is referencing.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 AnchoredTo(this AnchoredState state, int2 res) {
            switch (state) {
                case AnchoredState.BottomLeft:
                    return default;
                case AnchoredState.MiddleLeft:
                    return new int2(0, res.y / 2);
                case AnchoredState.TopLeft:
                    return new int2(0, res.y);
                case AnchoredState.BottomCenter:
                    return new int2(res.x / 2, 0);
                case AnchoredState.MiddleCenter:
                    return res / 2;
                case AnchoredState.TopCenter:
                    return new int2(res.x / 2, res.y);
                case AnchoredState.BottomRight:
                    return new int2(res.x, 0);
                case AnchoredState.MiddleRight:
                    return new int2(res.x, res.y / 2);
                case AnchoredState.TopRight:
                    return res;
                default:
                    throw new System.ArgumentException($"{state} is not a valid anchor!");
            }
        }

        /// <summary>
        /// Computes the relative anchor in local space given a resolution.
        /// </summary>
        /// <param name="res">The element's parent's current dimension</param><z
        /// <returns>The anchor in local space</returns>
        public static int2 AnchoredToRelative(this AnchoredState state, int2 res) {
            switch (state) {
                case AnchoredState.BottomLeft:
                    return -res / 2;
                case AnchoredState.MiddleLeft:
                    return new int2(-res.x / 2, 0);
                case AnchoredState.TopLeft:
                    return new int2(-res.x / 2, res.y / 2);
                case AnchoredState.BottomCenter:
                    return new int2(0, -res.y / 2);
                case AnchoredState.MiddleCenter:
                    return new int2(0, 0);
                case AnchoredState.TopCenter:
                    return new int2(0, res.y / 2);
                case AnchoredState.BottomRight:
                    return new int2(res.x / 2, -res.y / 2);
                case AnchoredState.MiddleRight:
                    return new int2(res.x / 2, 0);
                case AnchoredState.TopRight:
                    return res / 2;
                default:
                    throw new System.ArgumentException($"{state} is not a valid anchor!");
            }
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
