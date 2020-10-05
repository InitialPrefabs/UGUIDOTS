using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS.Transforms {

    /// <summary>
    /// Marks a messaging entity that the resolution has changed.
    /// </summary>
    public struct ResolutionEvent : IComponentData, IEquatable<ResolutionEvent> {
        public int2 Value;

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public bool Equals(ResolutionEvent other) {
            return other.Value.Equals(Value);
        }
    }

    public struct RescaleDimensionEvt : IComponentData { }
}
