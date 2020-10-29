using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS {

    /// <summary>
    /// Stores the cursor positions we use for comparisons.
    /// </summary>
    public struct Cursor : IBufferElementData {
        public float2 Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float3(Cursor value) => new float3(value.Value, 0);
    }
}
