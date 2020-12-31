using System;
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
            var index = Collection.Length - 1;
            var first = Collection[index];

            for (int i = index - 1; i >= 0; i--) {
                var current = Collection[i];

                if (current.Priority() < first.Priority()) {
                    first = current;
                    index = i;
                }
            }

            Collection.RemoveAt(index);
            return first;
        }

        public void Dispose() {
            if (Collection.IsCreated) {
                Collection.Dispose();
            }
        }

        public JobHandle Dispose(JobHandle inputDeps) {
            return Collection.Dispose(inputDeps);
        }
    }
}
