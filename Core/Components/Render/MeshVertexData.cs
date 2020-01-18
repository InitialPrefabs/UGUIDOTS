using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace UGUIDots.Render {

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertexData : IBufferElementData {
        public float2 Position;
        public float3 Normal;
        public float2 UVs;
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
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };
    }
}
