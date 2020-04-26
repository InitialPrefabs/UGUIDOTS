using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDots.Render {

    public unsafe class OrthographicRenderPass : ScriptableRenderPass {

        public unsafe struct RenderInstruction {
            public SubmeshKeyElement* Start;
            public MaterialPropertyBatch Batch;
            public Mesh Mesh;
        };

        public Queue<RenderInstruction> RenderInstructions { get; private set; }

        private string profilerTag;

        public OrthographicRenderPass(OrthographicRenderSettings settings) {
            profilerTag          = settings.ProfilerTag;
            base.renderPassEvent = settings.RenderPassEvt;
            RenderInstructions   = new Queue<RenderInstruction>();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (RenderInstructions.Count <= 0) {
                return;
            }

            var cmd = CommandBufferPool.Get(profilerTag);

            // TODO: Figure out how to use the profiling scope instead of the profiling sample.
            using (new ProfilingSample(cmd, profilerTag)) {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // Set the projection view matrix
                var proj = Matrix4x4.Ortho(0, Screen.width, 0, Screen.height, -100f, 100f);
                var view = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                cmd.SetViewProjectionMatrices(view, proj);

                var mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

                while (RenderInstructions.Count > 0) {
                    var dequed = RenderInstructions.Dequeue();
                    var keys   = dequed.Start;
                    var mesh   = dequed.Mesh;
                    var batch  = dequed.Batch.Value;

                    for (int i = 0; i < mesh.subMeshCount; i++) {
                        var matEntity = keys[i].MaterialEntity;

                        // TODO: I would like for the data to already be there without this check - need to revise soon
                        if (mgr.HasComponent<SharedMaterial>(matEntity)) {
                            var mat  = mgr.GetComponentData<SharedMaterial>(matEntity).Value;
                            var prop = batch[i];
                            cmd.DrawMesh(mesh, Matrix4x4.identity, mat, i, -1, prop);
                        }
                    }
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

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

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            renderer.EnqueuePass(Pass);
        }
    }
}
