using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public class CanvasScalerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var resolution = new int2(Screen.width, Screen.height);
            var cmdBuffer  = cmdBufferSystem.CreateCommandBuffer();

            Entities.WithNone<OnResolutionChangeTag>().ForEach(
                (Entity entity, in ScreenSpace c0, in Dimension c1, in ReferenceResolution c2) => {
                if (!c1.Value.Equals(resolution)) {
                    var logWidth  = math.log2((float)resolution.x / c2.Value.x);
                    var logHeight = math.log2((float)resolution.y / c2.Value.y);
                    var avg       = math.lerp(logWidth, logHeight, c2.WidthHeightWeight);
                    var scale     = math.pow(2, avg);

                    var screenSpace = new ScreenSpace {
                        Translation = resolution / 2,
                        Scale = scale
                    };

                    cmdBuffer.SetComponent(entity, screenSpace);
                    cmdBuffer.SetComponent(entity, new Dimension { Value = resolution });
                    cmdBuffer.AddComponent<OnResolutionChangeTag>(entity);
                }
            }).Run();
        }
    }
}
