using Unity.Entities;
using Unity.Jobs;

namespace UGUIDOTS.Transforms.Systems {
    [UpdateInGroup(typeof(UITransformConsumerGroup))]
    public class ConsumeChangeEvtSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities.ForEach((Entity entity, in ResolutionChangeEvt c0) => {
                cmdBuffer.DestroyEntity(entity.Index, entity);
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
