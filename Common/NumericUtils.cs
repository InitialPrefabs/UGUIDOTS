using Unity.Collections;
using Unity.Mathematics;

namespace UGUIDOTS.Common {

    public unsafe static class NumericUtils {

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
    }
}
