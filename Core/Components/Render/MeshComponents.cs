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
    /// Stores the root indices needed to generate triangles.
    /// </summary>
    public struct Index : IBufferElementData {
        public ushort Value;

        public static implicit operator Index(ushort value) => new Index { Value = value };
        public static implicit operator ushort(Index value) => value.Value;
    }

    /// <summary>
    /// Used to describe the submesh's index buffer and vertex buffer params.
    /// </summary>
    public struct SubmeshSliceElement : IBufferElementData {
        public int2 VertexSpan;
        public int2 IndexSpan;
    }

    /// <summary>
    /// Stores the key and material keys required to render. A value of Entity.Null indicates that the material or texture is 
    /// not used explicitly.
    /// </summary>
    public struct SubmeshKeyElement : IBufferElementData {
        public Entity TextureEntity;
        public Entity MaterialEntity;
    }

    /// <summary>
    /// Stores which slice of the vertices and indices that the particular element owns.
    /// </summary>
    public struct MeshDataSpan : IComponentData {
        public int2 VertexSpan;
        public int2 IndexSpan;
    }

    /// <summary>
    /// Stores entities that need to be rendered as a buffer element.
    /// </summary>
    public struct RenderElement : IBufferElementData {
        public Entity Value;
    }

    /// <summary>
    /// Stores the start index of the first entity in the batch, and the number of elements in the batch.
    /// </summary>
    public struct BatchedSpanElement : IBufferElementData {

        /// <summary>
        /// x: The first entity's index in the batch.
        /// y: The total number of elements in the batch.
        /// </summary>
        public int2 Value;

        public static implicit operator BatchedSpanElement(int2 value) => new BatchedSpanElement { Value = value };
        public static implicit operator int2(BatchedSpanElement value) => value.Value;
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
