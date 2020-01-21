using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    // TODO: Implement this
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MeshRenderSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}