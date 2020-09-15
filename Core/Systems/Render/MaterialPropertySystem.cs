using Unity.Entities;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(ResetMaterialGroup))]
    [DisableAutoCreation]
    public class ResetMaterialPropertySystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((MaterialPropertyBatch c0) => {
                for (int i = 0; i < c0.Value.Length; i++) {
                    c0.Value[i].Clear();
                }
            }).WithoutBurst().Run();
        }
    }

    [UpdateInGroup(typeof(UpdateMaterialGroup))]
    [DisableAutoCreation]
    public class UpdateTextureMaterialPropertySystem : SystemBase {
        protected override void OnUpdate() {

            var linkedTextures = GetComponentDataFromEntity<LinkedTextureEntity>(true);

            Entities.ForEach((MaterialPropertyBatch c0, DynamicBuffer<RenderElement> b0, 
                DynamicBuffer<BatchedSpanElement> b1) => {

                var spans          = b1.AsNativeArray();
                var renderEntities = b0.AsNativeArray();

                for (int i = 0; i < spans.Length; i++) {
                    var entityIndex  = spans[i].Value.x;
                    var renderEntity = renderEntities[entityIndex].Value;

                    if (linkedTextures.HasComponent(renderEntity)) {
                        var textureEntity = linkedTextures[renderEntity].Value;
                        var texture       = EntityManager.GetComponentData<SharedTexture>(textureEntity);

                        c0.Value[i].SetTexture(ShaderIDConstants.MainTex, texture.Value);
                    }
                }
            }).WithoutBurst().Run();
        }
    }
}
