using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDOTS.Core.Diagnostics.Systems {

    internal static class MeshUtils {
        internal static Mesh CreateQuad(int2 size, Color color) {
            var mesh = new Mesh();

            mesh.SetVertices(new Vector3[] {
                new float3(-size / 2, 0),
                new float3(size / 2 * new float2(-1, 1), 0),
                new float3(size / 2, 0),
                new float3(size / 2 * new float2(1, -1), 0)
            });

            mesh.SetColors(new Color[] {
                color, color, color, color
            });

            mesh.SetIndices(new int[] {
                0, 1, 2,
                0, 2, 3
            }, MeshTopology.Triangles, 0);

            return mesh;
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class OrthographicDebugRenderSystem : SystemBase {

        private CommandBuffer cmdBuffer;
        private Material material;
        private Mesh mesh;

        protected override void OnCreate() {
            cmdBuffer = CommandBufferPool.Get("Orthographic Render Debug");
            material  = Canvas.GetDefaultCanvasMaterial();
            mesh      = MeshUtils.CreateQuad(new int2(25), Color.cyan);
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

            Entities.ForEach((in ScreenSpace c0) => {
                cmdBuffer.DrawMesh(mesh, c0.AsMatrix(), material);
            }).WithoutBurst().Run();;
        }
    }
}
