using UGUIDOTS.Collections.Unsafe;
using NUnit.Framework;
using Unity.Collections;

namespace UGUIDOTS.Collections.Tests {

    public class UnsafeArrayTests {

        [Test]
        public void ValuesAreSet() {
            var values = new UnsafeArray<int>(10, Allocator.Temp);
            Assert.AreEqual(true, values.IsCreated);
            Assert.AreEqual(10, values.Length);

            for (int i = 0; i < values.Length; i++) {
                values[i] = 10 - i;
            }

            for (int i = 0; i < values.Length; i++) {
                Assert.AreEqual(10 - i, values[i]);
            }

            values.Dispose();

            Assert.AreEqual(false, values.IsCreated);
        }
    }
}
