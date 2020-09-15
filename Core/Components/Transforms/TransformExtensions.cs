using System.Runtime.CompilerServices;

using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDOTS.Transforms {

    public static class TransformExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this in float4x4 matrix) {
            return new float3(matrix.c0[0], matrix.c1[1], matrix.c2[2]) / matrix.c3[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this in LocalToWorld ltw) {
            var m = ltw.Value;
            return Scale(m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AverageScale(this in LocalToWorld ltw) {
            var scale = ltw.Scale();
            return math.csum(scale) / 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this in LocalToParent ltp) {
            var m = ltp.Value;
            return Scale(m);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AverageScale(this in LocalToParent ltp) {
            var scale = ltp.Scale();
            return math.csum(scale) / 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion LocalRotation(this in LocalToParent ltp) {
            return new quaternion(ltp.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 AsMatrix(this in LocalToWorldRect ltw) {
            return float4x4.TRS(new float3(ltw.Translation, 0), quaternion.identity, new float3(ltw.Scale, 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 AsMatrix(this in LocalToParentRect ltw) {
            return float4x4.TRS(new float3(ltw.Translation, 0), quaternion.identity, new float3(ltw.Scale, 0));
        }
    }
}
