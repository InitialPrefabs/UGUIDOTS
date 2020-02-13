using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    /// <summary>
    /// Used for internal look ups.
    /// </summary>
    public struct MeshIndex : ISystemStateComponentData, IEquatable<MeshIndex> {
        public int Value;

        public bool Equals(MeshIndex other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
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
