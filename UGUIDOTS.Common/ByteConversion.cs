using System.Runtime.CompilerServices;

namespace UGUIDOTS.Common {

    public unsafe static class ByteConversion {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillPointerWithBytes(this float value, byte* ptr) {
            uint cast = *(uint*)(&value);

            for (int i = 0; i < sizeof(float); i++) {
                ptr[i] = (byte)((cast >> i * 8) & 0xff);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(byte* ptr, int offset = 0) {
            uint cast = (uint)(
                ptr[0 + offset] << 0 |
                ptr[1 + offset] << 8 |
                ptr[2 + offset] << 16 |
                ptr[3 + offset] << 24);

            return *(float*)(&cast);
        }
    }
}
