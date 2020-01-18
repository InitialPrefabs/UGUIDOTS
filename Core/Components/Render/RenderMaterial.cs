using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

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
