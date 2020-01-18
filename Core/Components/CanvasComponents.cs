using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDots {

    /// <summary>
    /// If the canvas is set to the ScaleWithScreenSize, then this component should be attached to the Canvas component.
    /// </summary>
    public struct ReferenceResolution : IComponentData {
        public float2 Value;
    }

    /// <summary>
    /// The weight of whether the scaled canvas should try to match the width of the current window or its height.
    /// </summary>
    public struct WidthHeightRatio : IComponentData {
        public float Value;
    }

    /// <summary>
    /// Determines the priority of which chunk should be rendered first and so forth.
    /// </summary>
    public struct CanvasSortOrder : ISharedComponentData, IEquatable<CanvasSortOrder> {

        public int Value;

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public bool Equals(CanvasSortOrder other) {
            return other.Value == Value;
        }
    }

    /// <summary>
    /// Specifies that an the hierarchy is marked dirty and the cache holding the order of the entities that need to be
    /// rendered needs to be rebuilt.
    /// </summary>
    public struct DirtyTag : IComponentData { }
}
