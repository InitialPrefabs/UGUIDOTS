using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UGUIDots.Render.Systems.BuildMeshSystem;

namespace UGUIDots.Transforms.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(AnchorSystem))]
    public class StretchDimensionsSystem : JobComponentSystem {

        [BurstCompile]
        private struct StretchDimensionsJob : IJobForEachWithEntity<LocalToWorld, Dimensions, Stretch> {

            public int2 Resolution;
            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(Entity entity, int index, ref LocalToWorld c0, ref Dimensions c1, ref Stretch c2) {
                var scale = c0.Scale().xy;
                c1 = new Dimensions {
                    Value       = (int2)(Resolution / scale),
                };

                CmdBuffer.RemoveComponent<CachedMeshTag>(index, entity);
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();

            RequireSingletonForUpdate<ResolutionChangeEvt>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            var stretchDeps = new StretchDimensionsJob {
                Resolution = new int2(Screen.width, Screen.height),
                CmdBuffer = cmdBuffer
            }.Schedule(this, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(stretchDeps);
            return stretchDeps;
        }
    }
}
