using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDOTS.Collections {
    
    /// <summary>
    /// Identifier to state that a struct is a struct.
    /// </summary>
    public interface IStruct<T> where T : struct { }

    /// <summary>
    /// Defines that a struct has a priority and can be ordered.
    /// </summary>
    public interface IPrioritize<T> where T : struct {
        public int Priority();
    }

    // TODO: Construct a min heap version of this.
    /// <summary>
    /// A naive implementation of a priority queue using UnsafeList. With an O(1) insert time, but O(n) pull time.
    /// </summary>
    public struct UnsafeMinPriorityQueue<T> : IDisposable where T : unmanaged, IStruct<T>, IPrioritize<T> {

        public int Length {
            get => Collection.Length;
        }
        
        internal UnsafeList<T> Collection;

        public void Add(in T value) {
            Collection.Add(value);
        }

        /// <summary>
        /// Dequeues the first lowest priority element in the queue. O(n) time to perform this operation.
        /// </summary>
        public T Pull() {
            var index = 0;
            var priority = Collection[index].Priority();
            for (int i = 1; i < Length; i++) {
                var current = Collection[i].Priority();

                if (current > priority) {
                    priority = current;
                    index = i;
                }
            }

            var pulledValue = Collection[index];
            Collection.RemoveAt(index);
            return pulledValue;
        }

        public void Dispose() {
            if (Collection.IsCreated) {
                Collection.Dispose();
            }
        }
    }

    public unsafe struct PerThreadContainer<T> : IDisposable where T : unmanaged, IStruct<T> {
    
        [NativeDisableUnsafePtrRestriction]
        public UnsafeList<T>* Ptr;

        public int Length { get; private set; }

        private Allocator allocator;

        public PerThreadContainer(int threadCount, int capacity, Allocator allocator) {
            this.allocator = allocator;
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

                UnsafeUtility.Free(Ptr, allocator);
                Ptr = null;
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
