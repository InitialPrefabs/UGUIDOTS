using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static UGUIDots.Render.OrthographicRenderPass;

namespace UGUIDots.Render.Systems {

    [AlwaysSynchronizeSystem]
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

        protected unsafe override void OnUpdate() {
            DynamicBuffer<int> ints = default;

            Entities.WithStoreEntityQueryInField(ref renderQuery).

                ForEach((Mesh mesh, DynamicBuffer<SubmeshKeyElement> keys) => {
                    renderFeature.Pass.RenderInstructions.Enqueue(new RenderInstruction {
                        Start  = (SubmeshKeyElement*)keys.GetUnsafePtr(),
                        Mesh   = mesh,
                    }
                );
            }).WithoutBurst().Run();
        }
    }
}
