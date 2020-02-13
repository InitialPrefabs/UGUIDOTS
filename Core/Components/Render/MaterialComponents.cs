using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    /// <summary>
    /// Adds all the materials for each level of the mesh.
    /// </summary>
    public struct SubmeshMaterialIdxElement : IBufferElementData {
        public int Value;

        public static implicit operator SubmeshMaterialIdxElement(int value) => new SubmeshMaterialIdxElement { Value = value };
        public static implicit operator int(SubmeshMaterialIdxElement value) => value.Value;
    }

    /// <summary>
    /// Stores the material index determiend by the MaterialBin that the image uses.
    /// </summary>
    public struct MaterialKey : IComponentData, IEquatable<MaterialKey> {
        public int Value;

        public bool Equals(MaterialKey other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    // TODO: Deprecate this - I think adding a component object might make much more sense than an ISCD
    [Obsolete("RenderMaterial is deprecated")]
    public struct RenderMaterial : ISharedComponentData, IEquatable<RenderMaterial> {
        public Material Value;

        public override int GetHashCode() {
            if (Value != null) {
                return Value.GetHashCode();
            }
            return 0;
        }

        public bool Equals(RenderMaterial other) {
            return other.Value == Value;
        }
    }
}
