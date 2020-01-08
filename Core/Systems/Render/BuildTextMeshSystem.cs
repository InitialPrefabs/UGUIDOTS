using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildTextMeshSystem : JobComponentSystem {

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
