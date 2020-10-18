using Unity.Entities;

namespace UGUIDOTS.Render.Systems {

    public class CanvasMetadataDisposalSystem : SystemBase {

        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            commandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = commandBufferSystem.CreateCommandBuffer();

            Entities.WithNone<Vertex>().ForEach((Entity entity, ref ChildrenActiveMetadata c0) => {
                c0.Dispose();

                cmdBuffer.RemoveComponent<ChildrenActiveMetadata>(entity);
            }).Run();
        }
    }
}
