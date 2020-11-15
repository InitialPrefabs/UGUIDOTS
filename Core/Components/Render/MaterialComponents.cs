using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Render {

    public struct MaterialPropertyEntity : IComponentData {

        /// <summary>
        /// Stores the number of material property blocks to generate into the MaterialPropertyBatch.
        /// </summary>
        public int Count;
        public Entity Canvas;
    }

    [Serializable]
    public class SharedMaterial : IComponentData, IEquatable<SharedMaterial> {
        public Material Value;

        public override bool Equals(object obj) {
            return Equals((SharedMaterial)obj);
        }

        public override int GetHashCode() {
            return !ReferenceEquals(null, Value) ? Value.GetHashCode() : 0;
        }

        public bool Equals(SharedMaterial other) {
            return other.Value == Value;
        }

        public Material GetMaterial() {
            if (Value == null) {
                Value = Canvas.GetDefaultCanvasMaterial();
            }
            return Value;
        }
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

    /// <summary>
    /// Stores the associative entity that is representative of the material.
    /// </summary>
    public struct LinkedMaterialEntity : IComponentData {
        public Entity Value;
    }
}
