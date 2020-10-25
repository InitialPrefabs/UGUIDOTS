using System;
using Unity.Entities;

namespace UGUIDOTS.Core.Diagnostics {

    internal class DebugRenderCommand : IComponentData, IEquatable<DebugRenderCommand> {

        internal OrthographicDebugRenderFeature Value;

        public bool Equals(DebugRenderCommand other) {
            return other.Value == Value;
        }
    }
}
