using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDots.Render {

    public unsafe class OrthographicRenderPass : ScriptableRenderPass {

        public unsafe struct RenderInstruction {
            public SubmeshKeyElement* Start;
            public Mesh Mesh;
        };

        public Queue<RenderInstruction> RenderInstructions { get; private set; }

        private string                profilerTag;
        private MaterialPropertyBlock _tempBlock;

        public OrthographicRenderPass(OrthographicRenderSettings settings) {
            profilerTag          = settings.ProfilerTag;
            base.renderPassEvent = settings.RenderPassEvt;
            RenderInstructions   = new Queue<RenderInstruction>();
            _tempBlock           = new MaterialPropertyBlock();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (RenderInstructions.Count <= 0) {
                return;
            }

            // TODO: Need a better way to handle finding the camera
            var cmd = CommandBufferPool.Get(profilerTag);
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

                    for (int i = 0; i < mesh.subMeshCount; i++) {
                        var mat        = mgr.GetComponentObject<Material>(keys[i].MaterialEntity);
                        var textureKey = keys[i].TextureEntity;

                        _tempBlock.Clear();
                        if (textureKey != Entity.Null) {
                            _tempBlock.SetTexture(ShaderIDConstants.MainTex, 
                                mgr.GetComponentObject<Texture2D>(textureKey));
                        }

                        var m = Matrix4x4.identity;
                        cmd.DrawMesh(mesh, m, mat, i, -1, _tempBlock);
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
