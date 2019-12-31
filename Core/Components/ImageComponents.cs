using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

    /// <summary>
    /// Stores the default color that we intend to use for the image.
    /// </summary>
    public struct DefaultImageColor : IComponentData {
        public Color32 Value;
    }

    /// <summary>
    /// Stores the width and the height of the drawable image along with the key to image we want to use.
    /// </summary>
    public struct ImageDimensions : IComponentData, IEquatable<ImageDimensions> {
        public float2 Size;
        public int TextureKey;

        public override int GetHashCode() {
            return Size.GetHashCode() ^ TextureKey.GetHashCode();
        }

        public bool Equals(ImageDimensions other) {
            return other.Size.Equals(Size) && (other.TextureKey == TextureKey);
        }
    }

    public static class ImageExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Width(this ImageDimensions dim) {
            return (int)dim.Size.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Height(this ImageDimensions dim) {
            return (int)dim.Size.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Size(this ImageDimensions dim) {
            return new int2(dim.Width(), dim.Height());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Extents(this ImageDimensions dim) {
            return dim.Size / 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Size(this RectTransform transform) {
            return new float2(transform.rect.width, transform.rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Center(this ImageDimensions dim) {
            return new float2(dim.Extents());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 Int2Center(this ImageDimensions dim) {
            return new int2(dim.Extents());
        }
    }
}
