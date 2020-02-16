using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDots.Render {

    /// <summary>
    /// Used to describe the submesh's index buffer and vertex buffer params.
    /// </summary>
    public struct SubMeshSliceElement : IBufferElementData {
        public int2 VertexSpan;
        public int2 IndexSpan;
    }

    /// <summary>
    /// Stores the key and material keys required to render. A value of -1 indicates that the material or texture is 
    /// not used explicitly.
    /// </summary>
    public struct SubMeshKeyElement : IBufferElementData {
        public short TextureKey;
        public short MaterialKey;
    }

    /// <summary>
    /// Stores which slice of the vertices and indices that the mesh owns.
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
        public int2 Value;

        public static implicit operator BatchedSpanElement(int2 value) => new BatchedSpanElement { Value = value };
        public static implicit operator int2(BatchedSpanElement value) => value.Value;
    }

    /// <summary>
    /// Tags components as effectively dirty in the render group, this may be due to adding new UI elements 
    /// or shifting children around.
    /// </summary>
    [System.Obsolete]
    public struct UnsortedRenderTag : IComponentData { }

    /// <summary>
    /// Tags an entity to contain a render priority. Lower integer values mean less priority in rendering.
    /// </summary>
    [System.Obsolete]
    public struct RenderGroupID : IComponentData, IComparable<RenderGroupID> {
        public int Value;

        public int CompareTo(RenderGroupID other) {
            return Value.CompareTo(other.Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }
}
