using NUnit.Framework;
using UGUIDOTS.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UGUIDOTS {

    public class PerThreadContainerTests {

        struct Empty : IStruct<Empty> { }

        [Test]
        public void ElementsAddedToThreadContainer() {
            unsafe {
                var rand = new Random(1);
                var threadContainer = new PerThreadContainer<Empty>(2, 10, Allocator.Temp);

                var sizes = new UnsafeList<int>(4, Allocator.Temp);

                for (int i = 0; i < threadContainer.Length; i++) {
                    UnsafeList<Empty>* slice = threadContainer.Ptr + i;

                    var size = rand.NextInt(10);
                    sizes.Add(size);

                    for (int j = 0; j < size; j++) {
                        slice->Add(default);
                    }
                }

                for (int i = 0; i < threadContainer.Length; i++)
                {
                    UnsafeList<Empty>* slice = threadContainer.Ptr + i;
                    Assert.AreEqual(slice->Length, sizes[i]);
                }

                threadContainer.Dispose();
                sizes.Dispose();
            }
        }
    }
}
