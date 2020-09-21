using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateAfter(typeof(AnchorSystem))]
    public class StretchDimensionsSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<ResolutionChangeEvt>();
        }

        protected override void OnUpdate() {
            var cmdBuffer  = cmdBufferSystem.CreateCommandBuffer();
            var resolution = new float2(Screen.width, Screen.height);

            Entities.WithAll<Stretch>().ForEach((Entity entity, ref Dimensions c1, in ScreenSpace c2) => {
                var scale = c2.Scale;
                c1        = new Dimensions { Value = (int2)(resolution / scale) };

                // TODO: Tell the UI Element that the vertices need to be rebuilt.
                // cmdBuffer.AddComponent<BuildUIElementTag>(entity.Index, entity);
            }).WithBurst().Run();

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
