using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDOTS.Collections {

    public static class PriorityQueueExtensions {

        /// <summary>
        /// Extension function which generates a priority queue from the NativeArray.
        /// </summary>
        public static unsafe FixedPriorityQueue<T, U> AsPriorityQueue<T, U>(this NativeArray<T> values, U comparer) 
            where T : unmanaged, IStruct<T>, IPrioritize<T>, IComparable<T>
            where U : unmanaged, IComparer<T> {
            return new FixedPriorityQueue<T, U>(values.GetUnsafePtr(), values.Length, comparer);
        }

        /// <summary>
        /// Extension function which generates a priority queue from a NativeList.
        /// </summary>
        public static unsafe FixedPriorityQueue<T, U> AsPriorityQueue<T, U>(this NativeList<T> values, U comparer) 
            where T : unmanaged, IStruct<T>, IPrioritize<T>, IComparable<T>
            where U : unmanaged, IComparer<T> {
            return new FixedPriorityQueue<T, U>(values.GetUnsafePtr(), values.Length, comparer);
        }

    }

    public struct MinifyComparer<T> : IComparer<T> 
        where T : unmanaged, IStruct<T>, IPrioritize<T> {
        public int Compare(T x, T y) {
            return x.Priority().CompareTo(y.Priority());
        }
    }

    public struct MaximizeComparer<T> : IComparer<T> 
        where T : unmanaged, IStruct<T>, IPrioritize<T> {
        public int Compare(T x, T y) {
            return y.Priority().CompareTo(x.Priority());
        }
    }

    /// <summary>
    /// Defines that a struct has a priority and can be ordered.
    /// </summary>
    public interface IPrioritize<T> : IComparable<T> where T : struct {
        public int Priority();
    }

    /// <summary>
    /// Stores a pointer to a collection and treats the collection as a min priority queue.
    /// </summary>
    public unsafe struct FixedPriorityQueue<T, U> 
        where T : unmanaged, IStruct<T>, IPrioritize<T>, IComparable<T>
        where U : unmanaged, IComparer<T> {
        internal T* Ptr;

        /// <summary>
        /// Stores the internal index that tracks where we are in the collection.
        /// </summary>
        internal int Index;

        /// <summary>
        /// Stores an immutable property of the # of elements in the queue.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Checks if the queue is considered "empty." Empty means that the index we are tracking
        /// exceeds the total number of elements stored.
        /// </summary>
        public bool IsEmpty() {
            return Index >= Length;
        }

        /// <summary>
        /// Generic constructor to support any pointer with a defined collection size.
        /// </summary>
        public FixedPriorityQueue(void* ptr, int length, U comparer) {
            NativeSortExtension.Sort<T>((T*)ptr, length);

            Ptr = (T*)ptr;
            Length = length;
            Index = 0;
        }

        /// <summary>
        /// Looks at the current element on top of the queue.
        /// </summary>
        public T Peek() {
            return Ptr[Index];
        }

        /// <summary>
        /// Looks at the first most element on the queue and pops it off the queue.
        /// </summary>
        public T Pull() {
            CheckInRange(Index);
            var pulled = Ptr[Index];
            Index++;
            return pulled;
        }

        internal T this[int i] {
            get {
                CheckInRange(i);
                return Ptr[i];
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECK")]
        void CheckInRange(int index) {
            if (index < 0) {
                throw new ArgumentOutOfRangeException("Index cannot be less than 0");
            }

            if (index >= Length) {
                throw new ArgumentOutOfRangeException(string.Format("Index exceeds the Length: {0}", Length));
            }
        }
    }
}
