using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Render.Systems {
    [UpdateInGroup(typeof(MeshRenderGroup))]
    public class MeshCacheSystem : JobComponentSystem {

        private EntityQuery uncachedQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
