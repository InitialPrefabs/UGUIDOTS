﻿using Unity.Entities;

namespace UGUIDots.Controls.Messaging.Systems {

    // TODO: Rename the accompanying file
    [UpdateInGroup(typeof(MessagingConsumptionGroup))]
    public class ButtonMessageConsumerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            Dependency = Entities.ForEach((Entity entity, in ButtonMessageRequest c0) => {
                cmdBuffer.DestroyEntity(entity.Index, entity);
            }).WithBurst().Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }

    // TODO: Support the ButtonMessagePersistentPayload component which has a different life span
    // TODO: Change the job to an IJobChunk
    [UpdateInGroup(typeof(MessagingProductionGroup))]
    public class ButtonMessageProducerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        
        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();
            Dependency = Entities.ForEach((Entity entity, in ClickState c0, in ButtonMessageFramePayload c1) => {
                if (c0.Value) {
                    cmdBuffer.Instantiate(entity.Index, c1.Value);
                }
            }).ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
