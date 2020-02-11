using UGUIDots.Collections.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDots.Render {

    /// <summary>
    /// Stores all children entities which are batched into the canvas as submeshes.
    /// </summary>
    public unsafe struct MeshBatches : ISystemStateComponentData {
        public UnsafeArray<Entity> Elements;
        public UnsafeArray<int2> Spans;
    }
}
