using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    /// <summary>
    /// Removes the NonInteractableTag and actually marks the subgroup as disabled.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ReactiveSubgroupToggleSystem : SystemBase {
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            Dependency = Entities.WithAll<DisableRenderingTag, Disabled>().ForEach(
                (Entity entity, int entityInQueryIndex, DynamicBuffer<Child> b0) => {

                var children = b0.AsNativeArray();

                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;

                    cmdBuffer.RemoveComponent<Disabled>(current.Index, current);
                    cmdBuffer.AddComponent<Disabled>(current.Index, current);
                }

                cmdBuffer.AddComponent<Disabled>(entityInQueryIndex, entity);
                cmdBuffer.RemoveComponent<Disabled>(entityInQueryIndex, entity);
                cmdBuffer.RemoveComponent<DisableRenderingTag>(entityInQueryIndex, entity);
            }).ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
