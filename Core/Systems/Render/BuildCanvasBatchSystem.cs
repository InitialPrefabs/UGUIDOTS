using UGUIDots.Collections.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup))]
    public class BuildCanvasBatchSystem : JobComponentSystem {

        private struct BuildBatchSystem : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<MeshVertexData> MeshVertexData;

            [ReadOnly]
            public BufferFromEntity<TriangleIndexElement> Triangles;

            [ReadOnly]
            public ArchetypeChunkBufferType<RenderElement> MeshBatchType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                // TODO: Loop through the RenderEntities and fill the vertices
                // TODO: Add the data spans to the images and texts.
            }
        }

        private EntityQuery unbatchedCanvasGroup;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

            unbatchedCanvasGroup = GetEntityQuery(new EntityQueryDesc {
                All  = new [] { 
                    ComponentType.ReadOnly<RenderElement>()
                },
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
