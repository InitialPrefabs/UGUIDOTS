using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDOTS.Collections {

    public interface IStruct<T> where T : struct { }

    public unsafe struct PerThreadContainer<T> where T : unmanaged, IStruct<T> {
        public UnsafeList<T>* ThreadContainers;

        public PerThreadContainer(int threadCount, int capacity, Allocator allocator) {
            var size = UnsafeUtility.SizeOf<UnsafeList<T>>() * threadCount;

            ThreadContainers = (UnsafeList<T>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeList<T>>() * threadCount, 
                UnsafeUtility.AlignOf<UnsafeList<T>>(), 
                allocator);

            for (int i = 0; i < threadCount; i++) {
                ThreadContainers[i] = new UnsafeList<T>(capacity, allocator);
            }
        }
    }
}
