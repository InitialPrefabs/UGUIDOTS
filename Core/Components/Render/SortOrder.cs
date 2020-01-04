using System;
using Unity.Entities;

namespace UGUIDots.Render {

    /// <summary>
    /// Tags components as effectively dirty in the render group, this may be due to adding new UI elements 
    /// or shifting children around.
    /// </summary>
    public struct UnsortedRenderTag : IComponentData { }

    /// <summary>
    /// Tags an entity to contain a render priority. Lower integer values mean less priority in rendering.
    /// </summary>
    public struct RenderGroupID : IComponentData, IComparable<RenderGroupID> {
        public int Value;

        public int CompareTo(RenderGroupID other) {
            return Value.CompareTo(other.Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// Stores entities that need to be rendered as a buffer element.
    /// </summary>
    public struct RenderElement : IBufferElementData {
        public Entity Value;
    }
}
