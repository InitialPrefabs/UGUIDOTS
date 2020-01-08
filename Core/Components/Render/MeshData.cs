using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Render {

    public struct VertexElement : IBufferElementData {
        public float3 Value;

        public static implicit operator float3(VertexElement value) => value.Value;
        public static implicit operator VertexElement(float3 value) => new VertexElement { Value = value };
        public static implicit operator Vector3(VertexElement value) => value.Value;
    }

    public struct TriangleElement : IBufferElementData {
        public int Value;

        public static implicit operator TriangleElement(int value) => new TriangleElement { Value = value };
        public static implicit operator int(TriangleElement value) => value.Value;
    }
}
