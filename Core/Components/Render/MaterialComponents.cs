using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    public struct SharedMaterial : ISharedComponentData, IEquatable<SharedMaterial> {
        public Material Value;

        public override int GetHashCode() {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public bool Equals(SharedMaterial other) {
            return other.Value == Value;
        }
    }

    public struct LinkedMaterialEntity : IComponentData {
        public Entity Value;
    }
}
