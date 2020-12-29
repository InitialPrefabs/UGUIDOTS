using Unity.Mathematics;

namespace UGUIDOTS.Transforms {
    public static class OffsetConstants { 

        /// <summary>
        /// The number of pixels we want to shift the vertices.
        /// </summary>
        public static readonly float2 DisabledOffset = new float2(1 << 15);
    }
}
