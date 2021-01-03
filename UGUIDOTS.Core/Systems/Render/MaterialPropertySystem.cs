using Unity.Entities;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class ResetMaterialPropertySystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((MaterialPropertyBatch c0) => {
                for (int i = 0; i < c0.Value.Length; i++) {
                    c0.Value[i].Clear();
                }
            }).WithoutBurst().Run();
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(OrthographicRenderSystem))]
    public class UpdateTextureMaterialPropertySystem : SystemBase {

        protected override void OnUpdate() {

            Entities.ForEach((MaterialPropertyBatch c0, DynamicBuffer<SubmeshKeyElement> b0) => {
                var keys = b0.AsNativeArray();

                for (int i = 0; i < keys.Length; i++) {
                    var current = keys[i];
                    if (current.TextureEntity == Entity.Null) {
                        continue;
                    }

                    var texture = EntityManager.GetComponentData<SharedTexture>(current.TextureEntity).Value;

                    var materialPropertyBlock = c0.Value[i];
                    materialPropertyBlock.SetTexture(ShaderIDConstants.MainTex, texture);
                }
            }).WithoutBurst().Run();
        }
    }
}
