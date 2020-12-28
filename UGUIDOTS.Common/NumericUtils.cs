using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

[assembly: InternalsVisibleTo("UGUIDOTS.Common.Tests")]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToChar(this int value) {
            return (char)(value + CharOffset);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToCharArray(this float value, char* ptr, int digits) {
            int base10 = math.abs((int)value);
            var base10Digits = CountDigits(base10);
            var exponent = base10Digits - 1;

            var totalDigits = base10Digits + 1;

            // We want to store the negative sign
            if (value < 0) {
                totalDigits++;
            }

            var collection = new NativeList<char>(totalDigits, Allocator.Temp);

            while (exponent > 0) {
                var powerRaised = (int)math.pow(10, exponent);
                var digit = base10 / powerRaised;
                base10 = base10 % powerRaised;

                collection.Add((char)(digit + CharOffset));
                exponent--;
            }

            collection.Add(DecimalDelimiter);

            var fraction = math.abs(value) - base10;

            for (int i = 0; i < digits; i++) {
                fraction *= math.pow(10, (i + 1));

                var digit = (int)fraction;
                collection.Add((char)(digit + CharOffset));
            }
        }
    }

}
