using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static UGUIDots.Render.OrthographicRenderPass;

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

        protected unsafe override void OnUpdate() {
            Entities.WithStoreEntityQueryInField(ref renderQuery).

                ForEach((Mesh mesh, MaterialPropertyBatch batch, DynamicBuffer<SubmeshKeyElement> keys) => {
                    renderFeature.Pass.RenderInstructions.Enqueue(new RenderInstruction {
                        Start = (SubmeshKeyElement*)keys.GetUnsafePtr(),
                        Batch = batch,
                        Mesh  = mesh,
                    }
                );
            }).WithoutBurst().Run();
        }
    }
}
