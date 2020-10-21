using UGUIDOTS.Transforms;
using Unity.Entities;
using UnityEngine.Rendering;

namespace UGUIDOTS.Core.Diagnostics.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class OrthographicDebugRenderSystem : SystemBase {

        private CommandBuffer cmdBuffer;

        protected override void OnCreate() {
            cmdBuffer = CommandBufferPool.Get("Orthographic Render Debug");
        }
        
        protected override void OnStartRunning() {
            Entities.ForEach((DebugRenderCommand c0) => {
                c0.Value.CommandBuffer = cmdBuffer;
            }).WithoutBurst().Run();
        }

        protected override void OnUpdate() {
            Entities.ForEach((in ScreenSpace c0) => {
                // TODO: Generate a mesh - pretty much a dot.
                // Draw all the dots in screen space.
            }).WithoutBurst().Run();;
        }
    }
}
