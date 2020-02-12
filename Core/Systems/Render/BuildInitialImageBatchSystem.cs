using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup))]
    public class BuildInitialImageBatchSystem : JobComponentSystem {

        private struct BuildBatchSystem : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<MeshVertexData> MeshVertexData;

            [ReadOnly]
            public BufferFromEntity<TriangleIndexElement> Triangles;

            [ReadOnly]
            public ArchetypeChunkComponentType<MeshBatches> MeshBatchType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var batches = chunk.GetNativeArray(MeshBatchType);

                for (int i = 0; i < chunk.Count; i++) {
                    var batch    = batches[i];
                    var entities = batch.Elements;
                    var spans    = batch.Spans;

                    for (int k = 0; k < spans.Length; k++) {
                        //  TODO: Loop through all the spans
                    }
                }
            }
        }

        private EntityQuery unbuiltMeshQuery;

        protected override void OnCreate() {
            unbuiltMeshQuery = GetEntityQuery(new EntityQueryDesc {
                All  = new [] { ComponentType.ReadWrite<CanvasRenderer>() },
                None = new [] { ComponentType.ReadOnly<Mesh>() }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var meshBatchType = GetArchetypeChunkComponentType<MeshBatches>(true);
            return inputDeps;
        }
    }
}
