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

    // TODO: Define a much more descriptive functions - since these use "texture" space where 0,0 is the BL corner.
    public static class DimensionsExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 LocalSpaceOrigin(this in Dimensions dim) {
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 LocalSpaceUpperLeft(this in Dimensions dim) {
            var extents = dim.Extents();
            return dim.LocalSpaceOrigin() + new float2(-extents.x, extents.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Width(this in Dimensions dim) {
            return (int)dim.Value.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Height(this in Dimensions dim) {
            return (int)dim.Value.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Size(this in Dimensions dim) {
            return new int2(dim.Width(), dim.Height());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Extents(this in Dimensions dim) {
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
        public static float2 Center(this in Dimensions dim) {
            return new float2(dim.Extents());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Center(this in Dimensions dim) {
            return new int2(dim.Extents());
        }
    }
}
