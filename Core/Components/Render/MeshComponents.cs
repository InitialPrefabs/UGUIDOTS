using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDOTS.Render {

    [Serializable]
    public class SharedMesh : IComponentData, IEquatable<SharedMesh> {
        
        public Mesh Value;

        public override int GetHashCode() {
            if (!ReferenceEquals(null, Value)) {
                return Value.GetHashCode();
            }

            return 0;
        }

        public bool Equals(SharedMesh other) {
            return other.Value == Value;
        }
    }

    /// <summary>
    /// Stores all batched vertices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : IBufferElementData {
        public float3 Position;
        public float3 Normal;
        public float4 Color;
        public float2 UV1;
        public float2 UV2;
    }

    /// <summary>
    /// Stores the UI elements vertex element required for each mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [System.Obsolete]
    public struct LocalVertexData : IBufferElementData {
        public float3 Position;
        public float3 Normal;
        public float4 Color;
        public float2 UV1;
        public float2 UV2;

        public static implicit operator Vertex(LocalVertexData value) {
            return new Vertex {
                Position = value.Position,
                Normal   = value.Normal,
                Color    = value.Color,
                UV1      = value.UV1,
                UV2      = value.UV2
            };
        }
    }

    /// <summary>
    /// Stores the root indices needed to generate triangles.
    /// </summary>
    public struct Index : IBufferElementData {
        public ushort Value;

        public static implicit operator Index(ushort value) => new Index { Value = value };
        public static implicit operator ushort(Index value) => value.Value;
    }

    /// <summary>
    /// Stores the UI element's local indices to generate a triangle.
    /// </summary>
    [System.Obsolete]
    public struct LocalTriangleIndexElement : IBufferElementData {
        public ushort Value;

        public static implicit operator LocalTriangleIndexElement(ushort value) =>
            new LocalTriangleIndexElement { Value = value };
        public static implicit operator ushort(LocalTriangleIndexElement value) => value.Value;
    }

    /// <summary>
    /// Marks that an entity needs a Mesh associated with it.
    /// </summary>
    public struct AddMeshTag : IComponentData { }

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
