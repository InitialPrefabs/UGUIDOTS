using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

[assembly: InternalsVisibleTo("UGUIDOTS.Common.Tests")]
namespace UGUIDOTS.Common {

    public unsafe static class NumericUtils {

        /// <summary>
        /// To convert an integer representation to a char, we offset the integer by 48.
        /// </summary>
        internal const int NumericCharOffset = 48;

        /// <summary>
        /// The decimal delimiter found in any base 10 number.
        /// </summary>
        public const char DotDelimiter = '.';

        /// <summary>
        /// Extracts the total digits the integer value occupies.
        /// </summary>
        /// <param name="value">The integer to count digits</param>
        /// <returns>The total number of digits the integer occupies.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(int value) {
            var count = math.select(0, 1, value < 0);

            value = math.abs(value);
            while (value > 0) {
                value /= 10;
                count++;
            }

            return count;
        }

        /// <summary>
        /// Extracts the total digits the float value occupies.
        /// </summary>
        /// <param name="value">The float value to count</param>
        /// <param name="decimalPlaces">The # of decimal places to consider</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(float value, int decimalPlaces) {
            var absValue = math.abs(value);
            var base10 = (int)value;

            var count = CountDigits((int)value);
            var fraction = absValue - base10;

            if (fraction * math.pow(10, decimalPlaces) > 0) {
                count = count + decimalPlaces + 1;
            }

            return count;
        }

        /// <summary>
        /// Fills a character array pointer with the unicode equivalent of each digit.
        /// </summary>
        /// <param name="ptr">The character buffer to store into</param>
        /// <param name="length">The size of the buffer</param>
        /// <param name="count">The total number of counted digits</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToChars(this int value, char* ptr, in int length, out int count) {
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

        /// <summary>
        /// Converts a float to a character array. This conversion is not 100% accurate as the string representation, 
        /// due to floating point inaccuracies when rounding.
        /// </summary>
        /// <param name="value">The float value to convert</param>
        /// <param name="ptr">The pointer to the collection you want to fill</param>
        /// <param name="digits">The total number of digits of the fraction to account for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToChars(this float value, char* ptr, int digits) {
            int base10 = math.abs((int)value);
            var base10Digits = CountDigits(base10);
            var exponent = base10Digits - 1;

            var totalDigits = base10Digits + digits + 1;

            // We want to store the negative sign
            if (value < 0) {
                totalDigits++;
            }

            var collection = new NativeList<char>(totalDigits, Allocator.Temp);

            if (value < 0) {
                collection.Add('-');
            }

            var fraction = math.abs(value) - base10;

            while (exponent > -1) {
                var powerRaised = (int)math.pow(10, exponent);
                var digit = base10 / powerRaised;
                base10 = base10 % powerRaised;

                collection.Add((char)(digit + NumericCharOffset));
                exponent--;
            }

            collection.Add(DotDelimiter);

            for (int i = 0; i < digits; i++) {
                fraction = (fraction * 10) % 10;

                var digit = (int)fraction;
                collection.Add((char)(digit + NumericCharOffset));
            }

            UnsafeUtility.MemCpy(ptr, collection.GetUnsafePtr(), UnsafeUtility.SizeOf<char>() * totalDigits);
            collection.Dispose();
        }
    }
}
