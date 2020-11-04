using Unity.Entities;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateAfter(typeof(CursorCollisionSystem))]
    public class MarkToDisableSystem : SystemBase {

        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var commandBuffer = commandBufferSystem.CreateCommandBuffer();
            Entities.ForEach((in DynamicBuffer<TargetEntity> b0, in ButtonInvoked c0) => {
                if (c0.Value) {
                    var targets = b0.AsNativeArray();
                    for (int i = 0; i < targets.Length; i++) {
                        commandBuffer.AddComponent<ToggleActiveStateTag>(targets[i].Value);
                    }
                }
            }).Run();

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
