using System.Collections.Generic;
using UGUIDots.Collections.Runtime;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDots.Render {

    public class OrthographicRenderPass : ScriptableRenderPass {

        public Queue<(Mesh, Material, Matrix4x4, MaterialPropertyBlock)> InstructionQueue { get; private set; }
        public Queue<(NativeArray<SubMeshKeyElement>, Mesh)> RenderInstructions { get; private set; }

        private string                profilerTag;
        private Bin<Material>         materialBin;
        private Bin<Texture>          textureBin;
        private MaterialPropertyBlock _tempBlock;

        public OrthographicRenderPass(OrthographicRenderSettings settings) {
            profilerTag          = settings.ProfilerTag;
            base.renderPassEvent = settings.RenderPassEvt;
            InstructionQueue     = new Queue<(Mesh, Material, Matrix4x4, MaterialPropertyBlock)>();
            RenderInstructions   = new Queue<(NativeArray<SubMeshKeyElement>, Mesh)>();
            _tempBlock           = new MaterialPropertyBlock();

            MaterialBin.TryLoadBin("MaterialBin", out materialBin);
            TextureBin.TryLoadBin("TextureBin", out textureBin);
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

                while (InstructionQueue.Count > 0) {
                    var tuple    = InstructionQueue.Dequeue();
                    var mesh     = tuple.Item1;
                    var material = tuple.Item2;
                    var m        = tuple.Item3;
                    var block    = tuple.Item4;

                    for (int i = 0; i < mesh.subMeshCount; i++) {
                        cmd.DrawMesh(mesh, m, material, i, -i, block);
                    }
                }

                while (RenderInstructions.Count > 0) {
                    var dequed = RenderInstructions.Dequeue();
                    var keys   = dequed.Item1;
                    var mesh   = dequed.Item2;

                    for (int i = 0; i < mesh.subMeshCount; i++) {
                        var mat = materialBin.At(keys[i].MaterialKey);
                        Debug.Log(mat.name);
                        var textureKey = keys[i].TextureKey;

                        if (textureKey >= 0) {
                            _tempBlock.SetTexture(ShaderIDConstants.MainTex, textureBin.At(textureKey));
                        }

                        var m = Matrix4x4.identity;
                        cmd.DrawMesh(mesh, m, mat, i, 0, _tempBlock);
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
            ProfilerTag = "Orthographic Render Pass";
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
