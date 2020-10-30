using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS {

    public struct CursorElement {
        public float2 Position;
        public bool Pressed;
    }

    /// <summary>
    /// Stores the cursor positions we use for comparisons.
    /// </summary>
    public unsafe struct CursorBuffer : ISystemStateComponentData, IDisposable {

        internal CursorElement* Ptr;
        public readonly int Length;

        public CursorElement this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (index >= Length) {
                    throw new System.IndexOutOfRangeException($"Index: {index} must be between [0, {Length})!");
                }

                return Ptr[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                if (index >= Length) {
                    throw new System.IndexOutOfRangeException($"Index: {index} must be between [0, {Length})!");
                }
                CursorElement* element = Ptr + index;
                *element = value;
            }
        }

        public CursorBuffer(int capacity) {
            var size =  UnsafeUtility.SizeOf<float2>() * capacity;

            Ptr = (CursorElement*)UnsafeUtility.Malloc(
                size,
                UnsafeUtility.AlignOf<float2>(), 
                Allocator.Persistent);

            UnsafeUtility.MemSet(Ptr, 0, size);
            Length = capacity;
        }

        public void Dispose() {
            if (Ptr != null) {
                UnsafeUtility.Free(Ptr, Allocator.Persistent);
            }
        }
    }
}
