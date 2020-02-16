using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    // TODO: Implement this
    [UpdateInGroup(typeof(MeshRenderGroup))]
    [AlwaysSynchronizeSystem]
    public class OrthographicRenderSystem : JobComponentSystem {

        private EntityQuery renderQuery, renderCommandQuery;
        private OrthographicRenderFeature renderFeature;

        protected override void OnCreate() {
            renderQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<SubMeshKeyElement>(), ComponentType.ReadOnly<Mesh>() }
            });

            renderCommandQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<RenderCommand>() }
            });
        }

        protected override void OnStartRunning() {
            Entities.WithStoreEntityQueryInField(ref renderCommandQuery).ForEach((RenderCommand cmd) => {
                renderFeature = cmd.RenderFeature;
            }).WithoutBurst().Run();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Entities.WithStoreEntityQueryInField(ref renderQuery).WithoutBurst().
                ForEach((Mesh mesh, DynamicBuffer<SubMeshKeyElement> keys) => {
                renderFeature.Pass.RenderInstructions.Enqueue((keys.AsNativeArray(), mesh));
            }).Run();

            return inputDeps;
        }
    }
}
