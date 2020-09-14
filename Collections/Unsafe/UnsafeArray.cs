using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDOTS.Collections.Unsafe {

    // TODO: Allow for enumeration like foreach loops.
    public unsafe struct UnsafeArray<T> : IDisposable, IEquatable<UnsafeArray<T>> where T : unmanaged {

        public int Length { get; private set; }
        public bool IsCreated => _Ptr != null;

        [NativeDisableUnsafePtrRestriction]
        internal void* _Ptr;

        Allocator allocType;

        public UnsafeArray(int length, Allocator allocator) {
            var size  = UnsafeUtility.SizeOf<T>() * length;
            Length    = length;
            _Ptr      = UnsafeUtility.Malloc(size, UnsafeUtility.SizeOf<T>(), allocator);
            allocType = allocator;
        }

        public void Dispose() {
            if (IsCreated) {
                UnsafeUtility.Free(_Ptr, allocType);
                _Ptr   = null;
                Length = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef(int i) => ref ((T*)_Ptr)[i];

        public unsafe bool Equals(UnsafeArray<T> other) {
            return other.Length == Length && other._Ptr == _Ptr;
        }

        public override bool Equals(object other) {
            if (other == null) {
                return false;
            }
            return other is UnsafeArray<T> && Equals(other);
        }

        public override int GetHashCode() {
            return ((int)_Ptr * 397) ^ Length;
        }

        public T this[int i] {
            get => AsRef(i);
            set => AsRef(i) = value;
        }

        public static bool operator ==(in UnsafeArray<T> lhs, in UnsafeArray<T> rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(UnsafeArray<T> lhs, UnsafeArray<T> rhs) {
            return !lhs.Equals(rhs);
        }

        public static UnsafeArray<T> FromNativeArray(ref NativeArray<T> source, Allocator allocator) {
            var size = source.Length;
            var unsafeData = new UnsafeArray<T>(size, allocator);
            UnsafeUtility.MemCpy(unsafeData._Ptr, source.GetUnsafePtr(), size * UnsafeUtility.SizeOf<T>());
            return unsafeData;
        }

        public static UnsafeArray<T> FromNativeList(ref NativeList<T> source, Allocator allocator) {
            var size = source.Length;
            var unsafeData = new UnsafeArray<T>(size, allocator);
            UnsafeUtility.MemCpy(unsafeData._Ptr, source.GetUnsafePtr(), size * UnsafeUtility.SizeOf<T>());
            return unsafeData;
        }
    }
}
