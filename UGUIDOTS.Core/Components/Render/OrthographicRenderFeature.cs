using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDOTS.Render {

    public class OrthographicRenderFeature : ScriptableRendererFeature {

        private OrthographicRenderPass orthographic;

        public RenderPassEvent RenderPassEvent;

        public CommandBuffer CommandBuffer;

        public override void Create() {
            orthographic = new OrthographicRenderPass {
                CommandBuffer   = CommandBuffer,
                renderPassEvent = RenderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            renderer.EnqueuePass(orthographic);
        }
    }
}
