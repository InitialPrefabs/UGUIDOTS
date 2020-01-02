using System;
using Unity.Entities;

namespace UGUIDots.Render {

    /// <summary>
    /// Determines the priority of which chunk should be rendered first and so forth.
    /// </summary>
    public struct SortOrder : ISharedComponentData, IEquatable<SortOrder> {

        public int Value;

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public bool Equals(SortOrder other) {
            return other.Value == Value;
        }
    }
}
