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

    // TODO: Maybe an ISCD might not be the best idea b/c it splits chunks - ISCD might be much better
    /// <summary>
    /// Stores the material ID that the image uses.
    /// </summary>
    public struct MaterialID : ISharedComponentData, IEquatable<MaterialID> {
        public int Value;

        public bool Equals(MaterialID other) {
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
