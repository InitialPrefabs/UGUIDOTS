using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace UGUIDots.Render {

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertexData : IBufferElementData {
        public float3 Position;
        public float3 Normal;
        public float4 Color;
        public float2 UV1;
        public float2 UV2;
    }

    public struct TriangleIndexElement : IBufferElementData {
        public ushort Value;

        public static implicit operator TriangleIndexElement(ushort value) => 
            new TriangleIndexElement { Value = value };
        public static implicit operator ushort(TriangleIndexElement value) => value.Value;
    }

    public static class MeshVertexDataExtensions {

        /// <summary>
        /// Descriptor for the mesh generated for text.
        /// </summary>
        public static readonly VertexAttributeDescriptor[] VertexDescriptors = new [] {
            new VertexAttributeDescriptor(VertexAttribute.Position,  VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal,    VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color,     VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2)
        };
    }
}
