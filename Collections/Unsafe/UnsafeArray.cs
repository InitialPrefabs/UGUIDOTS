using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDots.Collections.Unsafe {

    public unsafe struct UnsafeArray<T> : IDisposable where T : unmanaged {

        public int Length { get; private set; }
        public bool IsCreated => Ptr != null;

        [NativeDisableUnsafePtrRestriction]
        internal void* Ptr;

        Allocator allocType;

        public UnsafeArray(int length, Allocator allocator) {
            var size = UnsafeUtility.SizeOf<T>() * length;
            Length    = length;
            Ptr       = UnsafeUtility.Malloc(size, UnsafeUtility.SizeOf<T>(), allocator);
            allocType = allocator;
        }

        public void Dispose() {
            if (IsCreated) {
                UnsafeUtility.Free(Ptr, allocType);
                Ptr    = null;
                Length = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef(int i) => ref ((T*)Ptr)[i];

        public T this[int i] {
            get {
                return AsRef(i);
            }
            set {
                AsRef(i) = value;
            }
        }
    }
}
