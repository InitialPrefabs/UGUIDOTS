using System.Runtime.CompilerServices;

using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDOTS.Transforms {

    public static partial class TransformExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Position(this in float4x4 m) {
            return m.c3.xyz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this in float4x4 matrix) {
            return new float3(matrix.c0[0], matrix.c1[1], matrix.c2[2]) / matrix.c3[3];
        }
    }
}
