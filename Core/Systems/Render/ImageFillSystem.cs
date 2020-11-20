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

                        // Fill type of 0 means that it is an axis type
                        block.SetInt(ShaderIDConstants.FillType, 0);
                        block.SetInt(ShaderIDConstants.Axis, (int)axisFillAmount.Axis);
                        block.SetInt(ShaderIDConstants.Flip, axisFillAmount.Flip ? 1 : 0);
                        block.SetFloat(ShaderIDConstants.Fill, axisFillAmount.FillAmount);
                    }
                }
            }).WithoutBurst().WithReadOnly(axisFillAmounts).Run();
        }
    }
}
