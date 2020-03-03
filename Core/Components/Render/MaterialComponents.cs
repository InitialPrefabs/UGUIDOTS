using System;
using Unity.Entities;

namespace UGUIDots.Render {

    /// <summary>
    /// Stores the material index determiend by the MaterialBin that the image uses.
    /// </summary>
    public struct MaterialKey : IComponentData, IEquatable<MaterialKey> {
        public short Value;

        public bool Equals(MaterialKey other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }
}
