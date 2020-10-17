using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS.Transforms {
    
    /// <summary>
    /// The local to world transformation.
    /// </summary>
    public struct ScreenSpace : IComponentData {
        public float2 Translation;
        public float2 Scale;
    }

    /// <summary>
    /// The local to parent transformation.
    /// </summary>
    public struct LocalSpace : IComponentData {
        public float2 Translation;
        public float2 Scale;
    }

    /// <summary>
    /// Replacement to Unity's Child buffer component.
    /// </summary>
    public struct ChildUI : IBufferElementData {
        public Entity Value;

        public static implicit operator ChildUI(Entity entity) => new ChildUI { Value = entity };
        public static implicit operator Entity(ChildUI value)  => value.Value;
    }

    public static partial class TransformExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AverageScale(this in ScreenSpace rect) {
            return (rect.Scale.x + rect.Scale.y) / 2f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 AsMatrix(this in ScreenSpace ltw) {
            return float4x4.TRS(new float3(ltw.Translation, 1), quaternion.identity, new float3(ltw.Scale, 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 AsMatrix(this in LocalSpace ltp) {
            return float4x4.TRS(new float3(ltp.Translation, 1), quaternion.identity, new float3(ltp.Scale, 1));
        }
    }
}
