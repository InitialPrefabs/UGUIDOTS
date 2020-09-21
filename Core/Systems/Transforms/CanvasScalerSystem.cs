using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public unsafe class CanvasScalerSystem : SystemBase {

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
            evtArchetype = EntityManager.CreateArchetype(typeof(ResolutionChangeEvt));

            resolution = new int2(Screen.width, Screen.height);
        }

        protected override void OnStartRunning() {
            var local = new int2(Screen.width, Screen.height);
            Entities.ForEach((ref ScreenSpace c0, in ReferenceResolution c1) => {
                var logWidth  = math.log2(local.x / c1.Value.x);
                var logHeight = math.log2(local.y / c1.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.WidthHeightWeight);
                var scale     = math.pow(2, avg);

                c0.Translation = local / 2;
                c0.Scale = scale;
            }).Run();
        }

        protected override void OnUpdate() {
            var local = new int2(Screen.width, Screen.height);

            if (local.Equals(resolution)) {
                return;
            }

            resolution = local;

            // TODO: Create a system which will initially scale the UI based on whatever the current resolution is.
            // TODO: The aspect ratio needs to be checked.

            Entities.ForEach((ref ScreenSpace c0, in ReferenceResolution c1)  => {
                var logWidth  = math.log2(local.x / c1.Value.x);
                var logHeight = math.log2(local.y / c1.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.WidthHeightWeight);
                var scale     = math.pow(2, avg);

                c0.Translation = local / 2;
                c0.Scale = scale;
            }).Run();

            // TODO: Produce an event so that the anchor and stretch image systems runs and adjusts based on different aspect ratios.
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();
            cmdBuffer.CreateEntity(evtArchetype);
        }
    }
}
