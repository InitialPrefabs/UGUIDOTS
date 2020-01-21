using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {

    // TODO: Implement this
    [UpdateInGroup(typeof(MeshRenderGroup))]
    [UpdateAfter(typeof(MeshCacheSystem))]
    public class MeshRenderSystem : JobComponentSystem {

        private MeshCacheSystem cache;

        protected override void OnCreate() {
            cache = World.GetOrCreateSystem<MeshCacheSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
