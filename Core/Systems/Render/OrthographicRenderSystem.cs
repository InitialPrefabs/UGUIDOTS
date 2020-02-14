using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    // TODO: Implement this
    [UpdateInGroup(typeof(MeshRenderGroup))]
    public class OrthographicRenderSystem : JobComponentSystem {

        protected override void OnCreate() {
            
        }

        protected override void OnStartRunning() {
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            inputDeps.Complete();

            return inputDeps;
        }
    }
}
