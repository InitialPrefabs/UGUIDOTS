using UGUIDots.Render;
using Unity.Entities;

namespace UGUIDots.Controls.Messaging.Systems {
    [UpdateInGroup(typeof(MessagingUpdateGroup))]
    public class ButtonCloseSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer         = cmdBufferSystem.CreateCommandBuffer();
            var disabled          = GetComponentDataFromEntity<Disabled>();
            var disabledRendering = GetComponentDataFromEntity<DisableRenderingTag>();

            Entities.WithAll<ButtonMessageRequest>().ForEach((in CloseTarget c0) => {
                var targetEntity = c0.Value;

                // When closing a group, the elements should be disabled and the elements should not render

                if (disabled.Exists(targetEntity)) {
                    cmdBuffer.RemoveComponent<Disabled>(targetEntity);
                    cmdBuffer.AddComponent<EnableRenderingTag>(targetEntity);
                } else {
                    cmdBuffer.AddComponent(targetEntity, new Disabled { });
                    cmdBuffer.AddComponent<DisableRenderingTag>(targetEntity);
                }
            }).Schedule(Dependency);
        }
    }
}
