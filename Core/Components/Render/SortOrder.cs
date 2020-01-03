using System;
using Unity.Entities;

namespace UGUIDots.Render {

    public struct RenderGroupID : IComponentData, IComparable<RenderGroupID> {
        public int Value;

        public int CompareTo(RenderGroupID other) {
            return Value.CompareTo(other.Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public struct RenderElement : IBufferElementData {
        public Entity Value;
    }
}
