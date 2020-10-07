using UGUIDOTS.Render;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    public class StretchDimensionsSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer  = cmdBufferSystem.CreateCommandBuffer();
            var resolution = new int2(Screen.width, Screen.height);

            Entities.WithAll<RescaleDimension, Stretch>().ForEach((Entity entity, ref Dimension c0) => {
                c0.Value = resolution;
                cmdBuffer.RemoveComponent<RescaleDimension>(entity);
            }).Run();

            if (!HasSingleton<ResolutionEvent>()) {
                return;
            }

            Entities.WithAll<Stretch, SpriteData>().ForEach(
                (Entity entity, ref Dimension c1, in ScreenSpace c2) => {

                var currentDim = c1.Value;
 
                float newAspectRatio = (float)resolution.x / resolution.y;
                float currentAspectRatio = (float)currentDim.x / currentDim.y;

                // Always rescale the dimension.
                c1 = new Dimension { Value = (int2)(resolution / c2.Scale ) };

                // TODO: This does not cover images that only stretch on 1 axis. I will need to write a function
                // TODO: which perform multiples checks on both axis if that's the case. 
                if (newAspectRatio != currentAspectRatio) {
                    // If the aspect ratios don't match, then mark the entity to rebuild.
                    cmdBuffer.AddComponent<UpdateSliceTag>(entity);
                }
            }).Run();

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
