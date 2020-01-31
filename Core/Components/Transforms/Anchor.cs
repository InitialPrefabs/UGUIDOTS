using System.Runtime.CompilerServices;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Transforms {

    /// <summary>
    /// Flags which state how a transform is anchored relative to the resolution of the screen.
    /// </summary>
    [System.Flags]
    public enum AnchoredState : byte {
        // Along the y axis
        TopRow    = 1 << 1,
        MiddleRow = 1 << 2,
        BottomRow = 1 << 3,

        // Along the x axis
        LeftColumn   = 1 << 4,
        MiddleColumn = 1 << 5,
        RightColumn  = 1 << 6,
    }

    public struct Anchor : IComponentData {
        public AnchoredState State;
        public float2 Distance;
    }

    public static class AnchorExtensions {

        /// <summary>
        /// Returns the adjusted anchored positions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 RecomputeAnchoredPosition(this Anchor anchor) {
            return anchor.State.AnchoredTo() - anchor.Distance;
        }

        /// <summary>
        /// Returns the position that the anchor is anchored to based on the current resolution.
        /// </summary>
        /// <param name="state">The current anchored state of the element.</param>
        /// <returns>The relative screenspace position that the anchor is referencing.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 AnchoredTo(this AnchoredState state) {
            var res = new int2(Screen.width, Screen.height);
            return state.AnchoredTo(res);
        }

        /// <summary>
        /// Returns the position that the anchor is supposedly anchored to.
        /// </summary>
        /// <param name="state">The current state of the anchor.</param>
        /// <param name="res">The current resolution we want to consider.</param>
        /// <returns>The relative screenspace position that the anchor is referencing.</returns>
        public static int2 AnchoredTo(this AnchoredState state, int2 res) {
            switch (state) {
                case AnchoredState.LeftColumn | AnchoredState.BottomRow:
                    return default;
                case AnchoredState.LeftColumn | AnchoredState.MiddleRow:
                    return new int2(0, res.y / 2);
                case AnchoredState.LeftColumn | AnchoredState.TopRow:
                    return new int2(0, res.y);
                case AnchoredState.MiddleColumn | AnchoredState.BottomRow:
                    return new int2(res.x / 2, 0);
                case AnchoredState.MiddleColumn | AnchoredState.MiddleRow:
                    return res / 2;
                case AnchoredState.MiddleColumn | AnchoredState.TopRow:
                    return new int2(res.x / 2, res.y);
                case AnchoredState.RightColumn | AnchoredState.BottomRow:
                    return new int2(res.x, 0);
                case AnchoredState.RightColumn | AnchoredState.MiddleRow:
                    return new int2(res.x, res.y / 2);
                case AnchoredState.RightColumn | AnchoredState.TopRow:
                    return res;
                default:
                    throw new System.ArgumentException($"{state} is not a valid state to anchor to!");
            }
        }

        public static AnchoredState ToAnchor(this RectTransform transform) {
            var min = transform.anchorMin;

            AnchoredState anchor = 0;
            switch (min.x) {
                case 0f:
                    anchor = AnchoredState.LeftColumn;
                    break;
                case 0.5f:
                    anchor = AnchoredState.MiddleColumn;
                    break;
                case 1f:
                    anchor = AnchoredState.RightColumn;
                    break;
                default:
                    throw new System.ArgumentException($"Cannot map: {min.x} as a valid AnchoredState!");
            }

            switch (min.y) {
                case 0f:
                    anchor |= AnchoredState.BottomRow;
                    break;
                case 0.5f:
                    anchor |= AnchoredState.MiddleRow;
                    break;
                case 1f:
                    anchor |= AnchoredState.RightColumn;
                    break;
                default:
                    throw new System.ArgumentException($"Cannot map: {min.y} as a valid AnchoredState!");
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
                    return AnchoredState.LeftColumn   | AnchoredState.BottomRow;
                case TextAlignmentOptions.Bottom:
                    return AnchoredState.MiddleColumn | AnchoredState.BottomRow;
                case TextAlignmentOptions.BottomRight:
                    return AnchoredState.RightColumn  | AnchoredState.BottomRow;
                case TextAlignmentOptions.Left:
                    return AnchoredState.LeftColumn   | AnchoredState.MiddleRow;
                case TextAlignmentOptions.Center:
                    return AnchoredState.MiddleColumn | AnchoredState.MiddleRow;
                case TextAlignmentOptions.Right:
                    return AnchoredState.RightColumn  | AnchoredState.MiddleRow;
                case TextAlignmentOptions.TopLeft:
                    return AnchoredState.LeftColumn   | AnchoredState.TopRow;
                case TextAlignmentOptions.Top:
                    return AnchoredState.MiddleColumn | AnchoredState.TopRow;
                case TextAlignmentOptions.TopRight:
                    return AnchoredState.RightColumn  | AnchoredState.TopRow;
                default:
                    throw new System.ArgumentException($"Cannot convert {anchor} to a valid AnchoredState!");
            }
        }
    }
}
