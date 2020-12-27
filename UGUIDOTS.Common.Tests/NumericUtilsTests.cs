using NUnit.Framework;

namespace UGUIDOTS.Common.Tests {

    public unsafe class NumericUtilTests {

        [Test]
        public void CountIntegerDigits() {
            var value = 123456789;
            var actual = NumericUtils.CountDigits(value);
            Assert.AreEqual(9, actual, "Digit count mismatch");
        }

        [Test]
        public void CountPositiveFloatDigits() {
            var value = 3.14f;

            char* buffer = stackalloc char[4];
            value.ToCharArray(buffer, 2);

            var stringValue = $"{value}";

            for (int i = 0; i < stringValue.Length; i++) {
                Assert.AreEqual(stringValue[i], buffer[i], "Mismatched char!");
            }
        }

        [Test]
        public void CharEqualsPositiveDigit() {
            int value = 1234567890;

            char* actual = stackalloc char[10];

            value.ToCharArray(actual, 10, out int count);

            string expected = value.ToString();

            for (int i = 0; i < count; i++) {
                Assert.AreEqual(expected[i], actual[i], "Mismatched digits");
            }
        }

        [Test]
        public void CharEqualsPositiveDigitWithExcess() {
            int value = 12345;

            char* actual = stackalloc char[15];
            value.ToCharArray(actual, 15, out int count);
            string exepected = value.ToString();

            for (int i = 0; i < count; i++) {

                Assert.AreEqual(exepected[i], actual[i], "Mismatched digits");
            }
        }

        [Test]
        public void CharEqualsNegativeDigits() {
            int value = -12345;

            char* actual = stackalloc char[6];
            value.ToCharArray(actual, 6, out int count);
            string expected = value.ToString();

            for (int i = 0; i < count; i++) {
                Assert.AreEqual(expected[i], actual[i], "Mismatch digits");
            }
        }

        [Test]
        public void CharEqualsNegativeDigitsWIthExcess() {
            int value = -1234569;

            char* actual = stackalloc char[15];
            value.ToCharArray(actual, 15, out int count);
            string expected = value.ToString();

            for (int i = 0; i < count; i++) {
                Assert.AreEqual(expected[i], actual[i], "Mismatch digits");
            }
        }
    }
}
