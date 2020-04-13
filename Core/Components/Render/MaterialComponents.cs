using System;
using Unity.Entities;
using Unity.Mathematics;
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

    public struct Float4MaterialPropertyParam : IBufferElementData {
        public readonly int ID;
        public float4 Value;

        public Float4MaterialPropertyParam(int id, float4 value) {
            ID = id;
            Value = value;
        }

        public Float4MaterialPropertyParam(int id) {
            ID = id;
            Value = default;
        }
    }

    public struct LinkedMaterialEntity : IComponentData {
        public Entity Value;
    }
}
