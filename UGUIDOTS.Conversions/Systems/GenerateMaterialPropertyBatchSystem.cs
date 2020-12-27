using UGUIDOTS.Render;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Conversions.Systems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GenerateMaterialPropertyBatchSystem : SystemBase {

        protected override void OnUpdate() {
            Entities.ForEach((Entity entity, DynamicBuffer<SubmeshKeyElement> b0, in MaterialPropertyEntity c0) => {
                var blocks = new MaterialPropertyBlock[b0.Length];
                for (int i = 0; i < b0.Length; i++) {
                    var current = b0[i];
                    var block = new MaterialPropertyBlock();

                    if (current.TextureEntity != Entity.Null && EntityManager.HasComponent<SharedTexture>(current.TextureEntity)) {
                        var texture = EntityManager.GetComponentData<SharedTexture>(current.TextureEntity).GetTexture();
                        block.SetTexture(ShaderIDConstants.MainTex, texture);
                    }

                    blocks[i] = block;
                }
                EntityManager.AddComponentData(c0.Canvas, new MaterialPropertyBatch { Value = blocks });
                EntityManager.DestroyEntity(entity);
            }).WithStructuralChanges().WithoutBurst().Run();
        }
    }
}
