using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Render {

    [Serializable]
    public class SharedTexture : IComponentData, IEquatable<SharedTexture> {

        public Texture Value;

        public override bool Equals(object obj) {
            return Equals((SharedTexture)obj);
        }

        public bool Equals(SharedTexture other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            var hash = 0;
            if (!ReferenceEquals(null, Value)) hash = Value.GetHashCode() ^ hash;
            return hash;
        }

        public Texture GetTexture() {
            if (Value == null) {
                Value = Texture2D.whiteTexture;
            }
            return Value;
        }
    }

    /// <summary>
    /// Stpres the entity that is linked with a Texture.
    /// </summary>
    public struct LinkedTextureEntity : IComponentData {
        public Entity Value;
    }
}
