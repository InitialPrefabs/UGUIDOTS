using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class CanvasScalerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery scaleQuery;
        private EntityArchetype evtArchetype;

        private int2 resolution;

        protected override void OnCreate() {
            scaleQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<ReferenceResolution>(),
                    ComponentType.ReadWrite<ScreenSpace>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            evtArchetype = EntityManager.CreateArchetype(typeof(ResolutionEvent));

            resolution = new int2(Screen.width, Screen.height);
        }

        protected override void OnStartRunning() {
            var local              = new int2(Screen.width, Screen.height);
            var currentAspectRatio = Screen.width / Screen.height;
            var cmdBuffer          = cmdBufferSystem.CreateCommandBuffer();

            Entities.ForEach((ref ScreenSpace c0, in ReferenceResolution c1) => {
                var logWidth  = math.log2(local.x / c1.Value.x);
                var logHeight = math.log2(local.y / c1.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.WidthHeightWeight);
                var scale     = math.pow(2, avg);

                c0.Translation = local / 2;
                c0.Scale = scale;
            }).Run();
            
            // Only stretched images in which the aspect ratio changes will need to be readjusted
            // This only accounts for things that are stretched on the xy axis.
            // Ultimately if we choose stretch on x or y, the image will just have to be rebuilt,
            // since the aspect ratio will never match.
            Entities.WithAll<Stretch>().ForEach((Entity entity, in Dimension c0) => {
                float aspectRatio = c0.Value.x / c0.Value.y;

                if (aspectRatio != currentAspectRatio) {
                    cmdBuffer.AddComponent<RescaleDimensionEvt>(entity);
                }
            }).Run();
        }

        protected override void OnUpdate() {
            var local = new int2(Screen.width, Screen.height);

            if (local.Equals(resolution)) {
                return;
            }

            resolution = local;

            Entities.ForEach((ref ScreenSpace c0, in ReferenceResolution c1)  => {
                var logWidth  = math.log2(local.x / c1.Value.x);
                var logHeight = math.log2(local.y / c1.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.WidthHeightWeight);
                var scale     = math.pow(2, avg);

                c0.Translation = local / 2;
                c0.Scale = scale;
            }).Run();

            // Generate the event which will cause everything to rebuild.
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();
            cmdBuffer.CreateEntity(evtArchetype);
        }
    }
}
