using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Transforms {

    public static class TransformExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this LocalToWorld ltw) {
            var m = ltw.Value;
            return new float3(m.c0[0], m.c1[1], m.c2[2]) / m.c3[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AverageScale(this LocalToWorld ltw) {
            var scale = ltw.Scale();
            return math.csum(scale) / 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(this LocalToParent ltp) {
            var m = ltp.Value;
            return new float3(m.c0[0], m.c1[1], m.c2[2]) / m.c3[3];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AverageScale(this LocalToParent ltp) {
            var scale = ltp.Scale();
            return math.csum(scale) / 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion LocalRotation(this LocalToParent ltp) {
            return new quaternion(ltp.Value);
        }
    }
}
