using System;
using Unity.Entities;
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

    /// <summary>
    /// Stores the key to the texture that needs to be displayed.
    /// </summary>
    public struct TextureKey : IComponentData {
        public int Value;
    }
}
