using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDOTS.Render {

    public class OrthographicRenderPass : ScriptableRenderPass {

        public CommandBuffer CommandBuffer;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
#if UNITY_EDITOR
            if (renderingData.cameraData.isSceneViewCamera || renderingData.cameraData.isPreviewCamera) {
                return;
            }
#endif

            if (CommandBuffer != null) {
                context.ExecuteCommandBuffer(CommandBuffer);
            }
        }
    }
}
