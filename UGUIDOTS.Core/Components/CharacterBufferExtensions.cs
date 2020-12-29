using System.Runtime.CompilerServices;
using UGUIDOTS.Common;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS {

    public unsafe static class CharacterBufferExtensions {
        
        /// <summary>
        /// Resizes the dynamic buffer of chars and replaces the buffer with all digits found in the integer.
        /// </summary>
        /// <param name="chars">The destination buffer</param>
        /// <param name="value">The integer to find all the values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResizeAndReplaceBufferWithChars(this DynamicBuffer<CharElement> chars, int value) {
            var length = NumericUtils.CountDigits(value) + math.select(0, 1, value < 0);

            if (chars.Length != length) {
                chars.Length = length;
            }

            value.ToChars((char*)chars.GetUnsafePtr(), chars.Length, out int count);
        }

        /// <summary>
        /// Resizes the dynamic buffer of chars and replaces the buffer with all digits found in the float.
        /// </summary>
        /// <param name="chars">The destination buffer</param>
        /// <param name="value">The float value to find all the chars.</param>
        /// <param name="decimalPlaces">The # of decimal places to account for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResizeAndReplaceBufferWithChars(
            this DynamicBuffer<CharElement> chars, float value, int decimalPlaces) {
            var length = NumericUtils.CountDigits(value, decimalPlaces);

            if (chars.Length != length) {
                chars.Length = length;
            }

            value.ToChars((char*)chars.GetUnsafePtr(), decimalPlaces);
        }
    }
}
