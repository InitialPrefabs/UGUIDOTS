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
}
