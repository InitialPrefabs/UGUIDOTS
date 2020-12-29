using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UGUIDOTS.Collections {

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

        public UnsafeMinPriorityQueue(Allocator allocator, int capacity) {
            Collection = new UnsafeList<T>(capacity, allocator);
        }

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

        public JobHandle Dispose(JobHandle inputDeps) {
            return Collection.Dispose(inputDeps);
        }

        public T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Collection[index]; }
        }
    }
}
