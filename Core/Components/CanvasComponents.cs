using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS {

    public struct RootCanvasReference : IComponentData {
        public Entity Value;

        public static RootCanvasReference Default() {
            return new RootCanvasReference { Value = Entity.Null };
        }
    }

    /// <summary>
    /// If the canvas is set to the ScaleWithScreenSize, then this component should be attached to the Canvas component.
    /// </summary>
    public struct ReferenceResolution : IComponentData {
        public float2 Value;
        public float WidthHeightWeight;
    }
}
