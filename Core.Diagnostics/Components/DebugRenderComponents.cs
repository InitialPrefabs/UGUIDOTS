using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Core.Diagnostics {

    internal struct DebugColor : IComponentData {
        internal Color32 Value;
    }

    internal class DebugRenderCommand : IComponentData, IEquatable<DebugRenderCommand> {

        internal OrthographicDebugRenderFeature Value;

        public bool Equals(DebugRenderCommand other) {
            return other.Value == Value;
        }
    }
}
