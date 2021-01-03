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
    }
}
