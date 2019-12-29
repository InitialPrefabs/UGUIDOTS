using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace UGUIDots.Transforms.Systems {

    [UpdateAfter(typeof(CanvasScalerSystem))]
    public class AnchorSystem : JobComponentSystem {

        private EntityQuery anchorQuery;

        protected override void OnCreate() {
            anchorQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadWrite<LocalToWorld>(), 
                    ComponentType.ReadOnly<Anchor>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
