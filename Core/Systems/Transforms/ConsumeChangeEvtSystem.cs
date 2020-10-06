using Unity.Entities;

namespace UGUIDOTS.Transforms.Systems {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ConsumeChangeEvtSystem : SystemBase {

        private EntityQuery resolutionQuery;

        protected override void OnCreate() {
            resolutionQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<ResolutionEvent>()
                }
            });

            RequireForUpdate(resolutionQuery);
        }

        protected override void OnUpdate() {
            EntityManager.DestroyEntity(resolutionQuery);
        }
    }
}
