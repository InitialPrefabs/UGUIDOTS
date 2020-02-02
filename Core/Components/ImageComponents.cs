using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Sprites;

namespace UGUIDots {

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

    public static class ColorExtensions {
        public static float4 ToFloat4(this Color32 color) {
            return new float4(color.r, color.g, color.b, color.a);
        }

        public static float4 ToNormalizedFloat4(this Color32 color) {
            return new float4(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
        }
    }

    /// <summary>
    /// Stores the key to the texture that needs to be displayed.
    /// </summary>
    public struct TextureKey : IComponentData {
        public int Value;
    }
}
