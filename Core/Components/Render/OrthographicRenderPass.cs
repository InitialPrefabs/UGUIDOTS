using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace UGUIDots.Render {

    public class OrthographicRenderPass : ScriptableRenderPass {

        private string profilerTag;
        private ProfilingSampler sampler;

        private CommandBuffer cmd;

        public OrthographicRenderPass(OrthographicRenderSettings settings) {
            base.renderPassEvent = settings.RenderPassEvt;
            profilerTag          = settings.ProfilerTag;
            sampler              = new ProfilingSampler(settings.ProfilerTag);

            Debug.Log("Created");
        }

        public CommandBuffer Init() {
            if (cmd != null) {
                return cmd;
            }

            cmd = CommandBufferPool.Get(profilerTag);

            return cmd;
        }

        public void Release() {
            if (cmd != null) {
                CommandBufferPool.Release(cmd);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
#if UNITY_EDITOR
            if (cmd == null) {
                return;
            }
#endif
            context.ExecuteCommandBuffer(cmd);
        }
    }
}
