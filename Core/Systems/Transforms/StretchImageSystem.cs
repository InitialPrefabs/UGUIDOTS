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
        }

        protected override void OnUpdate() {
            var cmdBuffer  = cmdBufferSystem.CreateCommandBuffer();
            var resolution = new int2(Screen.width, Screen.height);

            Entities.WithAll<RescaleDimensionEvt, Stretch>().ForEach((Entity entity, ref Dimension c0) => {
                c0.Value = resolution;
                cmdBuffer.RemoveComponent<RescaleDimensionEvt>(entity);
            }).Run();


            if (!HasSingleton<ResolutionChangeEvt>()) {
                return;
            }

            Entities.WithAll<Stretch>().ForEach((Entity entity, ref Dimension c1, in ScreenSpace c2) => {
                var scale = c2.Scale;
                c1        = new Dimension { Value = (int2)(resolution / scale) };

                // TODO: Tell the UI Element that the vertices need to be rebuilt.
                // cmdBuffer.AddComponent<BuildUIElementTag>(entity.Index, entity);
            }).WithBurst().Run();

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
