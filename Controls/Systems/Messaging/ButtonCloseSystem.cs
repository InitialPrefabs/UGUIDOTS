using UGUIDots.Render;
using UGUIDots.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Controls.Messaging.Systems {

    [UpdateInGroup(typeof(MessagingUpdateGroup))]
    public class CloseButtonSystem : SystemBase {

        private struct ToggleJob { 

            public EntityCommandBuffer CmdBuffer;

            [ReadOnly]
            public ComponentDataFromEntity<Disabled> Disabled;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            public ComponentDataFromEntity<ChildrenActiveMetadata> Metadata;

            public void Execute(in CloseTarget c0) {
                var targetEntity = c0.Value;

                // Check the metadata of the entity and update its state
                var root         = HierarchyUtils.GetRoot(targetEntity, Parents);
                var activeStates = Metadata[root];

                if (activeStates.Value.TryGetValue(targetEntity, out bool isActive)) {
                    isActive = !isActive;
                    activeStates.Value[targetEntity] = isActive;
                }

                if (isActive && Disabled.Exists(targetEntity)) {
                    CmdBuffer.RemoveComponent<Disabled>(targetEntity);
                    CmdBuffer.AddComponent(targetEntity, new EnableRenderingTag { });
                }

                if (!isActive && !Disabled.Exists(targetEntity)) {
                    CmdBuffer.AddComponent<Disabled>(targetEntity);
                    CmdBuffer.AddComponent<DisableRenderingTag>(targetEntity);

                    RecurseChildrenAndDisabled(targetEntity);
                }
            }

            private void RecurseChildrenAndDisabled(Entity entity) {
                if (Children.Exists(entity)) {
                    var children = Children[entity].AsNativeArray();

                    for (int i = 0; i < children.Length; i++) {
                        var child = children[i].Value;

                        CmdBuffer.AddComponent<Disabled>(child);
                        RecurseChildrenAndDisabled(child);
                    }
                }
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var job       = new ToggleJob {
                CmdBuffer = cmdBufferSystem.CreateCommandBuffer(),
                Disabled  = GetComponentDataFromEntity<Disabled>(true),
                Parents   = GetComponentDataFromEntity<Parent>(true),
                Metadata  = GetComponentDataFromEntity<ChildrenActiveMetadata>(false),
                Children  = GetBufferFromEntity<Child>(true)
            };

            Dependency = Entities.WithAll<ButtonMessageRequest>().ForEach((in CloseTarget c0) => {
                job.Execute(c0);
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
