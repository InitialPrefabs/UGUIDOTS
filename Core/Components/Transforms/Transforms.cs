using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Transforms {

    public static class TransformExtensions {

        public static float3 Scale(this LocalToWorld ltw) {
            var m = ltw.Value;

            return new float3(m.c0[0], m.c1[1], m.c2[2]) / m.c3[3];
        }
    }
}
