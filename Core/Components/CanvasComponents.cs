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
    public struct WidthHeightWeight : IComponentData {
        public float Value;
    }
}
