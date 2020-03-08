using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class ButtonColorStateSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery buttonColorQuery;

        protected override void OnCreate() {
            buttonColorQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<AppliedColor>() , ComponentType.ReadOnly<ColorStates>()
                },
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            Dependency = Entities.WithStoreEntityQueryInField(ref buttonColorQuery).
                ForEach((Entity entity, in AppliedColor c0, in ColorStates c1, in ClickState c2) => {

                bool delta = false;
                Color32 color = default;

                // TODO: Redo how button clicks are registered.

                if (delta) {
                    cmdBuffer.SetComponent(entity.Index, entity, new AppliedColor { Value = color });
                    cmdBuffer.AddComponent<UpdateVertexColorTag>(entity.Index, entity);
                }
            }).WithBurst().ScheduleParallel(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
