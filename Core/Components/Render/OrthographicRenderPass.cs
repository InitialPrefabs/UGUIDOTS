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
#if UNITY_EDITOR
            if (CommandBuffer == null) {
                return;
            }
#endif
            context.ExecuteCommandBuffer(CommandBuffer);
        }
    }
}
