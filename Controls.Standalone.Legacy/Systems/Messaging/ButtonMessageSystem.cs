using Unity.Entities;

namespace UGUIDOTS.Controls.Messaging.Systems {

    [DisableAutoCreation]
    // TODO: Rename the accompanying file
    public class ButtonMessageConsumerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities.ForEach((Entity entity, in ButtonMessageRequest c0) => {
                cmdBuffer.DestroyEntity(entity.Index, entity);
            }).WithBurst().Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }

    [DisableAutoCreation]
    public class ButtonMessageProducerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        
        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter();
            Dependency = Entities.ForEach((Entity entity, in ClickState c0, in ButtonMessageFramePayload c1) => {
                if (c0.Value) {
                    var msgEntity = cmdBuffer.Instantiate(entity.Index, c1.Value);
                    cmdBuffer.AddComponent<ButtonMessageRequest>(entity.Index, msgEntity);
                }
            }).ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
