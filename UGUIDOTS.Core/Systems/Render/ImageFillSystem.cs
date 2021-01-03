using Unity.Entities;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(OrthographicRenderSystem))]
    public class ImageFillSystem : SystemBase {

        protected override void OnUpdate() {
            var axisFillAmounts   = GetComponentDataFromEntity<AxisFillAmount>(true);
            var radialFillAmounts = GetComponentDataFromEntity<RadialFillAmount>(true);

            Entities.ForEach((MaterialPropertyBatch c0, in DynamicBuffer<RenderElement> b0) => {
                var renderedElements = b0.AsNativeArray().AsReadOnly();
                var materialBlocks = c0.Value;

                for (int i = 0; i < renderedElements.Length; i++) {
                    var entity = renderedElements[i].Value;

                    var block = materialBlocks[i];

                    if (axisFillAmounts.HasComponent(entity)) {
                        var axisFillAmount = axisFillAmounts[entity];

                        block.SetInt(ShaderIDConstants.FillType, (int)FillType.Axis);
                        block.SetInt(ShaderIDConstants.Axis, (int)axisFillAmount.Axis);
                        block.SetInt(ShaderIDConstants.Flip, axisFillAmount.Flip ? 1 : 0);
                        block.SetFloat(ShaderIDConstants.Fill, axisFillAmount.FillAmount);
                    }

                    if (radialFillAmounts.HasComponent(entity)) {
                        var radialFillAmount = radialFillAmounts[entity];

                        block.SetInt(ShaderIDConstants.FillType, (int)FillType.Radial);
                        block.SetFloat(ShaderIDConstants.Angle, radialFillAmount.Angle);
                        block.SetFloat(ShaderIDConstants.Arc1, radialFillAmount.Arc1);
                        block.SetFloat(ShaderIDConstants.Arc2, radialFillAmount.Arc2);
                    }
                }
            }).
                WithoutBurst().
                WithReadOnly(axisFillAmounts).
                WithReadOnly(radialFillAmounts).
                Run();
        }
    }
}
