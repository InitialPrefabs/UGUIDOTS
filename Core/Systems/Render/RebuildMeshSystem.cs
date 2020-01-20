using UGUIDots.Conversions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RebuildMeshSystem : JobComponentSystem {

        private struct GenerateMeshJob : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public Arhc

            [ReadOnly]
            public EntityManager Manager;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                }
            }
        }

        private EntityQuery imageQuery;

        protected override void OnCreate() {
            imageQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] { ComponentType.ReadOnly<Dimensions>(), ComponentType.ReadOnly<MeshRebuildTag>() },
                Any = new[] { ComponentType.ReadOnly<DynamicRenderTag>() },
                None = new[] { ComponentType.ReadOnly<CharElement>() }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
