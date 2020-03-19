using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDots.Render {

    /// <summary>
    /// Used to describe the submesh's index buffer and vertex buffer params.
    /// </summary>
    public struct SubmeshSliceElement : IBufferElementData {
        public int2 VertexSpan;
        public int2 IndexSpan;
    }

    /// <summary>
    /// Stores the key and material keys required to render. A value of -1 indicates that the material or texture is 
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
        public int2 Value;

        public static implicit operator BatchedSpanElement(int2 value) => new BatchedSpanElement { Value = value };
        public static implicit operator int2(BatchedSpanElement value) => value.Value;
    }
}
