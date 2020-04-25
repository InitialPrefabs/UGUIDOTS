using System.Runtime.CompilerServices;
using Unity.Entities;

namespace UGUIDots.Render {

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
    public struct FillAmount : IComponentData {
        public float Amount;
        public FillType Type;
    }

    public static class FillExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FillTypeAsInt(ref this FillAmount value) {
            return (int)value.Type;
        }
    }
}
