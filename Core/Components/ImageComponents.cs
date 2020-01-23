using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

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
