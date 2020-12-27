using UGUIDOTS.Render;
using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDOTS.Core.Diagnostics.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public class OrthographicDebugRenderSystem : SystemBase {

        private CommandBuffer cmdBuffer;
        private Material material;
        private Mesh mesh;
        private MaterialPropertyBlock tempBlock;

        protected override void OnCreate() {
            cmdBuffer = CommandBufferPool.Get("Orthographic Render Debug");
            material  = Canvas.GetDefaultCanvasMaterial();
            mesh      = MeshUtils.CreateQuad(new int2(25), Color.white);
            tempBlock = new MaterialPropertyBlock();
        }
        
        protected override void OnStartRunning() {
            Entities.ForEach((DebugRenderCommand c0) => {
                c0.Value.CommandBuffer = cmdBuffer;
            }).WithoutBurst().Run();
        }

        protected override void OnUpdate() {
            cmdBuffer.Clear();
            cmdBuffer.SetViewProjectionMatrices(
                Matrix4x4.Ortho(0, Screen.width, 0, Screen.height, -100f, 100), 
                Matrix4x4.identity);

            Entities.ForEach((in ScreenSpace c0, in AppliedColor c1) => {
                tempBlock.Clear();

                tempBlock.SetColor(ShaderIDConstants.Color, c1.Value);
                cmdBuffer.DrawMesh(mesh, c0.AsMatrix(), material, 0, -1, tempBlock);
            }).WithoutBurst().Run();
        }
    }
}
