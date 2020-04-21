using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    // TODO: Turn this into a managed component data instead to avoid unnecessary chunk splitting
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

    /// <summary>
    /// Stores the index of the material property that the rendered element needs to update.
    /// </summary>
    public struct MaterialPropertyIndex : IComponentData {
        public ushort Value;
    }

    /// <summary>
    /// Stores a collection of material property blocks which is batched to a single entity.
    /// </summary>
    public class MaterialPropertyBatch : IComponentData, IEquatable<MaterialPropertyBatch> {
        public MaterialPropertyBlock[] Value;

        public override bool Equals(object obj) {
            return Equals((MaterialPropertyBatch)obj);
        }

        public bool Equals(MaterialPropertyBatch other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    public struct LinkedMaterialEntity : IComponentData {
        public Entity Value;
    }
}
