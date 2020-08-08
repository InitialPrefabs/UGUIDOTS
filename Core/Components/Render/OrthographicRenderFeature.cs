using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDots.Render {

    [System.Serializable]
    public class OrthographicRenderSettings {
        public string ProfilerTag;
        public RenderPassEvent RenderPassEvt;

        public OrthographicRenderSettings() {
            ProfilerTag   = "Orthographic Render Pass";
            RenderPassEvt = RenderPassEvent.AfterRenderingPostProcessing;
        }
    }

    public class OrthographicRenderFeature : ScriptableRendererFeature {

        public OrthographicRenderPass Pass { get; private set; }
        public OrthographicRenderSettings Settings = new OrthographicRenderSettings();

        public override void Create() {
            Pass = new OrthographicRenderPass(Settings);
            Pass.renderPassEvent = Settings.RenderPassEvt;
        }

        public CommandBuffer InitCommandBuffer() {
            if (Pass == null) {
                Create();
            }

            return Pass.Init();
        }

        public void ReleaseCommandBuffer() {
            Pass.Release();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            renderer.EnqueuePass(Pass);
        }
    }
}
