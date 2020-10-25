using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDOTS.Collections {

    public interface IStruct<T> where T : struct { }

    public unsafe struct PerThreadContainer<T> : IDisposable where T : unmanaged, IStruct<T> {

        [NativeDisableUnsafePtrRestriction]
        public UnsafeList<T>* Ptr;

        public int Length { get; private set; }

        public PerThreadContainer(int threadCount, int capacity, Allocator allocator) {
            Length = threadCount;
            var size = UnsafeUtility.SizeOf<UnsafeList<T>>() * threadCount;

            Ptr = (UnsafeList<T>*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<UnsafeList<T>>() * threadCount, 
                UnsafeUtility.AlignOf<UnsafeList<T>>(), 
                allocator);

            for (int i = 0; i < threadCount; i++) {
                Ptr[i] = new UnsafeList<T>(capacity, allocator);
            }
        }

        public void Dispose() {
            if (Ptr != null) {
                for (int i = 0; i < Length; i++) {
                    UnsafeList<T>* current = Ptr + i;
                    if (current->IsCreated) {
                        current->Dispose();
                    }
                }

                Ptr->Dispose();
            }
        }

        public void Reset() {
            for (int i = 0; i < Length; i++) {
                UnsafeList<T>* current = Ptr + i;
                current->Clear();
            }
        }
    }
}
