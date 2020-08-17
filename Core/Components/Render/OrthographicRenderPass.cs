using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDots.Render {

    public class OrthographicRenderPass : ScriptableRenderPass {

        public CommandBuffer CommandBuffer;

        public ProfilingSampler Sampler;

        public void Release() {
            if (CommandBuffer != null) {
                CommandBufferPool.Release(CommandBuffer);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            using (new ProfilingScope(CommandBuffer, Sampler)) {
                if (CommandBuffer != null) {
                    context.ExecuteCommandBuffer(CommandBuffer);
                }
            }
        }
    }
}
