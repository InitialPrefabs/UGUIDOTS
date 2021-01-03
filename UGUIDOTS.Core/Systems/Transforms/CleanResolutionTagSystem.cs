using Unity.Entities;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public class CleanResolutionTagSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery changedCanvasQuery;

        protected override void OnCreate() {
            changedCanvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<OnResolutionChangeTag>(), ComponentType.ReadOnly<ReferenceResolution>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();
            cmdBuffer.RemoveComponent<OnResolutionChangeTag>(changedCanvasQuery);
        }
    }
}
