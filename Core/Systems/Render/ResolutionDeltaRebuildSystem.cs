using UGUIDots.Transforms;
using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(UITransformUpdateGroup))]
    public class ResolutionDeltaRebuildSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<ResolutionChangeEvt>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            Dependency = Entities.WithNone<BuildUIElementTag>().ForEach((Entity entity, in SpriteData c0) => {
                cmdBuffer.AddComponent(entity, new BuildUIElementTag { });
            }).Schedule(Dependency);

            Dependency = Entities.WithNone<BuildUIElementTag>().ForEach((Entity entity, in DynamicBuffer<CharElement> b0) => {
                cmdBuffer.AddComponent(entity, new BuildUIElementTag { });
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
