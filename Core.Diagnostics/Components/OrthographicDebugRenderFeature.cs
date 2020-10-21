using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDOTS.Core.Diagnostics {

    internal class OrthographicDebugRenderPass : ScriptableRenderPass {

        internal CommandBuffer CommandBuffer;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (CommandBuffer == null || renderingData.cameraData.isSceneViewCamera) {
                return;
            }
            context.ExecuteCommandBuffer(CommandBuffer);
        }
    }

    internal class OrthographicDebugRenderFeature : ScriptableRendererFeature {

        public RenderPassEvent RenderPassEvent;
        internal CommandBuffer CommandBuffer;

        private OrthographicDebugRenderPass debugPass;

        public override void Create() {
            debugPass = new OrthographicDebugRenderPass {
                CommandBuffer = CommandBuffer,
                renderPassEvent = RenderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            renderer.EnqueuePass(debugPass);
        }
    }
}
