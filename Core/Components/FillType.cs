using System.Runtime.CompilerServices;
using Unity.Entities;

namespace UGUIDOTS {

    public enum Axis {
        X = 0,
        Y = 1
    }

    /// <summary>
    /// Fill types only support x/y axis based fills. Radial support will be coming 
    /// in later.
    /// </summary>
    public enum FillType : int {
        RightToLeft = 0,
        LeftToRight = 1,
        BottomToTop = 2,
        TopToBottom = 3,
    }

    /// <summary>
    /// Stores a normalized value between 0 and 1 that shows how much of the image is filled.
    /// </summary>
    [System.Obsolete]
    public struct FillAmount : IComponentData {
        public float Amount;
        public FillType Type;
    }

    /// <summary>
    /// Stores the fill amount, the axis this effects, and whether or not the direction of the 
    /// axis should be flipped.
    /// </summary>
    public struct AxisFillAmount : IComponentData {

        /// <summary>
        /// Stores a value inclusive between [0, 1].
        /// </summary>
        public float FillAmount;

        /// <summary>
        /// Stores which axis this should effect.
        /// </summary>
        public Axis Axis;

        /// <summary>
        /// Should the direction be flipped? 
        /// For the x axis by default we fill from <b>left -> right</b>, while flipping will fill 
        /// from <b>right -> left</b>.
        /// For the y axis by default we will from <b>bottom -> top</b>, while flipping will fill 
        /// from <b>top -> bottom</b>.
        /// </summary>
        public bool Flip;
    }

    public static class FillExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FillTypeAsInt(ref this FillAmount value) {
            return (int)value.Type;
        }
    }
}
