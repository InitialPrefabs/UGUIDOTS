using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UGUIDOTS {

    /// <summary>
    /// Stores the sprite's UVs, padding, and minimum size.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteData : IComponentData {
        public float4 InnerUV;
        public float4 OuterUV;
        public float4 Padding;
        public float2 MinSize;

        public static SpriteData FromSprite(Sprite sprite) {
            if (sprite) {
                return new SpriteData {
                    InnerUV = DataUtility.GetInnerUV(sprite),
                    OuterUV = DataUtility.GetOuterUV(sprite),
                    Padding = DataUtility.GetPadding(sprite),
                    MinSize = DataUtility.GetMinSize(sprite)
                };
            }
            return default;
        }
    }

    // TODO: This is a temporary solution for individual sprite data - may not work with packed atlases...
    /// <summary>
    /// Stores the sprite's texture resolution so that we can use it for scaling
    /// </summary>
    public struct DefaultSpriteResolution : IComponentData {
        public int2 Value;
    }

    /// <summary>
    /// Stores the intended color to apply to the entity.
    /// </summary>
    public struct AppliedColor : IComponentData, IEquatable<AppliedColor> {
        public Color32 Value;

        public bool Equals(AppliedColor other) {
            return other.Value.Equals(Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// Stores various color states that need to be applied to the image.
    /// </summary>
    public struct ColorStates : IComponentData { 
        public Color32 DefaultColor, HighlightedColor, PressedColor, DisabledColor;

        public static ColorStates FromColorBlock(ColorBlock block) {
            return new ColorStates {
                HighlightedColor = block.highlightedColor,
                PressedColor     = block.pressedColor,
                DisabledColor    = block.disabledColor,
                DefaultColor     = block.normalColor
            };
        }
    }

    /// <summary>
    /// A proxy to have an equatable version of Color32.
    /// </summary>
    public struct EquatableColor32 : IEquatable<EquatableColor32> {
        public byte R, B, G, A;

        public bool Equals(EquatableColor32 other) {
            return other.A == A && other.G == G && other.B == B && other.R == R;
        }

        public static implicit operator EquatableColor32(Color other) => new EquatableColor32 {
            R = (byte)(other.r * 255),
            G = (byte)(other.g * 255),
            B = (byte)(other.b * 255),
            A = (byte)(other.a * 255)
        };

        public static implicit operator EquatableColor32(Color32 other) => new EquatableColor32 {
            R = other.r,
            G = other.g,
            B = other.b,
            A = other.a
        };
    }

    public static class ColorExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 ToFloat4(this in Color32 color) {
            return new float4(color.r, color.g, color.b, color.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 ToNormalizedFloat4(this in Color32 color) {
            return new float4(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
        }
    }
}
