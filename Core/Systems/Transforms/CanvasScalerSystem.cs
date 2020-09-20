using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateInGroup(typeof(UITransformConsumerGroup))]
    public class ConsumeChangeEvtSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities.ForEach((Entity entity, in ResolutionChangeEvt c0) => {
                cmdBuffer.DestroyEntity(entity.Index, entity);
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(UITransformProducerGroup))]
    public unsafe class CanvasScalerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery scaleQuery;
        private EntityArchetype evtArchetype;

        private int2 resolution;

        protected override void OnCreate() {
            scaleQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<ReferenceResolution>(),
                    ComponentType.ReadWrite<LocalToWorldRect>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            evtArchetype = EntityManager.CreateArchetype(typeof(ResolutionChangeEvt));

            resolution = new int2(Screen.width, Screen.height);
        }

        protected override void OnStartRunning() {
            var local = new int2(Screen.width, Screen.height);
            Entities.ForEach((ref LocalToWorldRect c0, in ReferenceResolution c1) => {
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
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            if (local.Equals(resolution)) {
                return;
            }

            Entities.ForEach((ref LocalToWorldRect c0, in ReferenceResolution c1)  => {
                var logWidth  = math.log2(local.x / c1.Value.x);
                var logHeight = math.log2(local.y / c1.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.WidthHeightWeight);
                var scale     = math.pow(2, avg);

                c0.Translation = local / 2;
                c0.Scale = scale;
            }).Run();

            // TODO: Produce an event so that the anchor and stretch image systems runs and adjusts based on different aspect ratios.
        }
    }
}
