using Unity.Entities;

namespace UGUIDots.Render.Systems {

    /// <summary>
    /// Removes the DisabledTag and actually marks the subgroup as disabled.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ReactiveSubgroupToggleSystem : SystemBase {

        private EntityQuery disabledUpdatesQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            disabledUpdatesQuery = GetEntityQuery(new EntityQueryDesc {
                None = new [] {
                    ComponentType.ReadOnly<LocalTriangleIndexElement>(), ComponentType.ReadOnly<LocalVertexData>()
                },
                Any = new [] { 
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadOnly<Disabled>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            var disabled = GetComponentDataFromEntity<Disabled>(true);
            var update   = GetComponentDataFromEntity<UpdateVertexColorTag>(true);

            Dependency = Entities.WithStoreEntityQueryInField(ref disabledUpdatesQuery).
                ForEach((Entity entity, int entityInQueryIndex) => {
                cmdBuffer.RemoveComponent<UpdateVertexColorTag>(entityInQueryIndex, entity);
            }).ScheduleParallel(Dependency);

            Dependency = Entities.WithAll<EnableRenderingTag>().ForEach((Entity entity, int entityInQueryIndex) => {
                cmdBuffer.RemoveComponent<EnableRenderingTag>(entityInQueryIndex, entity);
            }).ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
