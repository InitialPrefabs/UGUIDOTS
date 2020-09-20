using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS.Transforms {

    public struct LocalToWorldRect : IComponentData {
        public float2 Translation;
        public float2 Scale;
    }

    public struct LocalToParentRect : IComponentData {
        public float2 Translation;
        public float2 Scale;
    }

    public static partial class TransformExtensions {

        public static float AverageScale(this in LocalToWorldRect rect) {
            return (rect.Scale.x + rect.Scale.y) / 2f;
        }
    }
}
