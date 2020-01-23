using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

    /// <summary>
    /// Represents the bounding box which may be used to generate the quads for images. For text based entities,
    /// this struct would be defined as the "bounding box" of the text for word wrapping/truncation.
    /// </summary>
    public struct Dimensions : IComponentData, IEquatable<Dimensions> {
        public int2 Value;

        public bool Equals(Dimensions other) {
            return other.Value.Equals(Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public static class DimensionsExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Width(this Dimensions dim) {
            return (int)dim.Value.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Height(this Dimensions dim) {
            return (int)dim.Value.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Size(this Dimensions dim) {
            return new int2(dim.Width(), dim.Height());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Extents(this Dimensions dim) {
            return dim.Value / 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Size(this RectTransform transform) {
            return new float2(transform.rect.width, transform.rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Size(this RectTransform transform) {
            return new int2((int)transform.rect.width, (int)transform.rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Center(this Dimensions dim) {
            return new float2(dim.Extents());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Center(this Dimensions dim) {
            return new int2(dim.Extents());
        }
    }
}
