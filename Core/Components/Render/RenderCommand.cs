using System;
using Unity.Entities;

namespace UGUIDots.Render {

    public class RenderCommand : IComponentData, IEquatable<RenderCommand> {

        public OrthographicRenderFeature RenderFeature;

        public bool Equals(RenderCommand other) {
            return other.RenderFeature == RenderFeature;
        }

        public override int GetHashCode() {
            var hash = 0;
            if (ReferenceEquals(RenderFeature, null)) hash ^= RenderFeature.GetHashCode();
            return hash;
        }
    }
}
