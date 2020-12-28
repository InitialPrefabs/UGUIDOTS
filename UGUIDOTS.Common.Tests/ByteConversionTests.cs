using NUnit.Framework;
using UnityEngine;

namespace UGUIDOTS.Common.Tests {

    public unsafe class ByteConversionTests {

        [Test]
        public void FloatConversionIsEquivalent() {
            var randomFloat = Random.Range(float.MinValue, float.MaxValue);
            byte* ptr = stackalloc byte[4];

            randomFloat.FillPointerWithBytes(ptr);
            var actual = ByteConversion.ToFloat(ptr);

            Assert.AreEqual(randomFloat, actual);
        }

        [Test]
        public void SignedBitIsNegative() {
            var value = -24.30f;

            byte* ptr = stackalloc byte[4];
            value.FillPointerWithBytes(ptr);

            byte first = ptr[0];

            string printValue = "";

            for (int i = 0; i < 4; i++) {
                printValue = $"{printValue}{System.Convert.ToString(ptr[i], 2)}";
            }

            UnityEngine.Debug.Log($"Binary: {printValue}, Little Endian: {System.BitConverter.IsLittleEndian}");

            Assert.AreEqual(1, first >> 7);
        }

        [Test]
        public void SignedBitIsPositive() {
            var value = 8.0f;

            byte* ptr = stackalloc byte[4];
            value.FillPointerWithBytes(ptr);

            byte first = ptr[0];

            var bytes = System.BitConverter.GetBytes(value);

            var accumulated = 0;

            for (int i = 0; i < 4; i++) {
                var v = ptr[i] << i * 8;

                accumulated |= v;
                Debug.Log($"Custom: {System.Convert.ToString(ptr[i], 2)}, DOTNET: {System.Convert.ToString(bytes[i], 2)}");
            }


            UnityEngine.Debug.Log($"Binary: {System.Convert.ToString(accumulated, 2)}, Little Endian: {System.BitConverter.IsLittleEndian}, {ByteConversion.ToFloat(ptr)}");

            Assert.AreEqual(0, first >> 7);
        }
    }
}
