using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    [DisableAutoCreation]
    [UpdateInGroup(typeof(UITransformUpdateGroup))]
    public class ResolutionDeltaRebuildSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();
            Dependency = Entities.ForEach((Entity e, in BuildUIElementTag c0) => {
                cmdBuffer.RemoveComponent<BuildUIElementTag>(e);
            }).Schedule(Dependency);

            Dependency = Entities.ForEach((Entity e) => {
                cmdBuffer.AddComponent<BuildTextTag>(e);
            }).WithNone<BuildTextTag>().Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
