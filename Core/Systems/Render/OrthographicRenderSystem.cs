using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshRenderGroup))]
    public class OrthographicRenderSystem : SystemBase {

        private EntityQuery renderQuery, renderCommandQuery;
        private OrthographicRenderFeature renderFeature;

        protected override void OnCreate() {
            renderQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<SubmeshKeyElement>(), ComponentType.ReadOnly<Mesh>() }
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

        protected override void OnUpdate() {
            Entities.WithStoreEntityQueryInField(ref renderQuery).WithoutBurst().
                ForEach((Mesh mesh, DynamicBuffer<SubmeshKeyElement> keys) => {
                var submeshKeys = keys.AsNativeArray();
                renderFeature.Pass.RenderInstructions.Enqueue((submeshKeys, mesh));
            }).Run();
        }
    }
}
