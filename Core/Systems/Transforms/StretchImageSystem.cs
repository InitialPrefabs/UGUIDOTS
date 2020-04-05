using UGUIDots.Render;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    [UpdateInGroup(typeof(UITransformUpdateGroup))]
    [UpdateAfter(typeof(AnchorSystem))]
    public class StretchDimensionsSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<ResolutionChangeEvt>();
        }

        protected override void OnUpdate() {
            var cmdBuffer  = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();
            var resolution = new float2(Screen.width, Screen.height);

            Dependency = Entities.ForEach((Entity entity, ref Dimensions c1, in LocalToWorld c0, in Stretch c2) => {
                var scale = c0.Scale().xy;
                c1        = new Dimensions { Value = (int2)(resolution / scale) };
                cmdBuffer.AddComponent<BuildUIElementTag>(entity.Index, entity);
            }).WithBurst().ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
