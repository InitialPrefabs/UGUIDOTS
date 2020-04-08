using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    [Serializable]
    public struct SharedMaterial : ISharedComponentData, IEquatable<SharedMaterial> {
        public Material Value;

        public override int GetHashCode() {
            return !ReferenceEquals(null, Value) ? Value.GetHashCode() : 0;
        }

        public bool Equals(SharedMaterial other) {
            return other.Value == Value;
        }
    }

    public struct LinkedMaterialEntity : IComponentData {
        public Entity Value;
    }
}
