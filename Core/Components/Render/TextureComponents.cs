using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    [Serializable]
    public struct SharedTexture : ISharedComponentData, IEquatable<SharedTexture> {

        public Texture Value;

        public bool Equals(SharedTexture other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            var hash = 0;
            if (!ReferenceEquals(null, Value)) hash = Value.GetHashCode() ^ hash;
            return hash;
        }
    }

    /// <summary>
    /// Stpres the entity that is linked with a Texture
    /// </summary>
    public struct LinkedTextureEntity : IComponentData {
        public Entity Value;
    }
}
