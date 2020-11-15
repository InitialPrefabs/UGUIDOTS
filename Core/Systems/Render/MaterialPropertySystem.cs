using Unity.Entities;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(OrthographicRenderSystem))]
    public class ImageFillSystem : SystemBase {
        protected override void OnUpdate() {
            var axisFillAmounts = GetComponentDataFromEntity<AxisFillAmount>(true);

            Entities.ForEach((MaterialPropertyBatch c0, in DynamicBuffer<RenderElement> b0) => {
                var renderedElements = b0.AsNativeArray().AsReadOnly();
                var materialBlocks = c0.Value;

                for (int i = 0; i < renderedElements.Length; i++) {
                    var entity = renderedElements[i].Value;

                    if (axisFillAmounts.HasComponent(entity)) {
                        var block = materialBlocks[i];
                        var axisFillAmount = axisFillAmounts[entity];
                        block.SetFloat(ShaderIDConstants.Fill, axisFillAmount.FillAmount);
                        block.SetInt(ShaderIDConstants.Axis, (int)axisFillAmount.Axis);
                        block.SetInt(ShaderIDConstants.Flip, axisFillAmount.Flip ? 1 : 0);
                    }
                }
            }).WithoutBurst().WithReadOnly(axisFillAmounts).Run();
        }
    }

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
