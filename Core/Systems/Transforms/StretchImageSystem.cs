using UGUIDots.Render;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    [UpdateInGroup(typeof(UITransformUpdateGroup))]
    [UpdateAfter(typeof(AnchorSystem))]
    public class StretchDimensionsSystem : JobComponentSystem {

        [BurstCompile]
        private struct StretchDimensionsJob : IJobForEachWithEntity<LocalToWorld, Dimensions, Stretch> {

            public float2 Resolution;
            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(Entity entity, int index, ref LocalToWorld c0, ref Dimensions c1, ref Stretch c2) {
                var scale = c0.Scale().xy;
                var newDimensions = (int2)(Resolution / scale);

                c1 = new Dimensions {
                    Value = newDimensions,
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
                Resolution = new float2(Screen.width, Screen.height),
                CmdBuffer = cmdBuffer
            }.Schedule(this, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(stretchDeps);
            return stretchDeps;
        }
    }
}
