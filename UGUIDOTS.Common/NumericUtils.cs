using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

[assembly: InternalsVisibleTo("")]
namespace UGUIDOTS.Common {

    public unsafe static class NumericUtils {

        /// <summary>
        /// To convert an integer representation to a char, we offset the integer by 48.
        /// </summary>
        internal const int CharOffset = 48;

        public const char DecimalDelimiter = '.';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CountDigits(int value) {
            var count = 0;
            while (value > 0) {
                value /= 10;
                count++;
            }

            return count;
        }

        /// <summary>
        /// Fills a character array pointer with the unicode equivalent of each digit.
        /// </summary>
        /// <param name="ptr">The character buffer to store into</param>
        /// <param name="length">The size of the buffer</param>
        /// <param name="count">The total number of counted digits</param>
        public static void ToCharArray(this int value, char* ptr, in int length, out int count) {
            var offset = value < 0 ? 0 : -1;

            value = math.abs(value);

            var stack = new NativeList<char>(Allocator.Temp);
            do {
                var mod = value % 10;
                value /= 10;

                // Convert to the unicode numerical equivalent, the last digit should go into the last value
                stack.Add((char)(mod + 48));
            } while (value != 0);

            for (int i = 0; i < stack.Length; i++) {
                var flipped = stack.Length + offset - i;

                if (flipped < length) {
                    ptr[flipped] = stack[i];
                }
            }

            count = stack.Length;

            if (offset >= 0) {
                ptr[0] = '-';
                count++;
            }

            stack.Dispose();
        }

        // TODO: Figure out how to count the number of digits in a floating point #.
        public static void ToCharArray(this float value, char* ptr, int digits) {
            var base10         = (int)value;
            var fraction       = value - base10;
            var fractionBase10 = fraction * (1 << digits);
            var baseLength     = CountDigits(base10) + digits + 1;

            var stack = new NativeList<char>(baseLength, Allocator.Temp);

            while (base10 > 0) {
                var mod = base10 % 10;
                base10 /= 10;
                stack.Add((char)(mod + 48));
            }

            stack.Add(DecimalDelimiter);

            while (fraction > 0) {
                var mod = fractionBase10 % 10;
                fractionBase10 /= 10;

                stack.Add((char)(mod + CharOffset));
            }
        }
    }

}
