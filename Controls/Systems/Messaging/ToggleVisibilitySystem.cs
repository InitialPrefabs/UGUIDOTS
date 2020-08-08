using UGUIDots.Render;
using UGUIDots.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Controls.Messaging.Systems {

    [UpdateInGroup(typeof(MessagingUpdateGroup))]
    public class ToggleVisibilitySystem : SystemBase {

        [BurstCompile]
        private struct ToggleJob { 

            public EntityCommandBuffer CmdBuffer;

            [ReadOnly]
            public ComponentDataFromEntity<Disabled> Disabled;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public ComponentDataFromEntity<ShowButtonType> ShowButtons;

            [ReadOnly]
            public ComponentDataFromEntity<ToggleButtonType> ToggleButtons;

            [ReadOnly]
            public ComponentDataFromEntity<CloseButtonType> CloseButtons;

            [ReadOnly]
            public BufferFromEntity<LocalVertexData> LocalVertices;

            public ComponentDataFromEntity<ChildrenActiveMetadata> Metadata;

            public void Execute(Entity msgEntity, DynamicBuffer<CloseTarget> b0) {
                var buffer = b0.AsNativeArray();

                for (int i = 0; i < buffer.Length; i++) {
                    var targetEntity = buffer[i].Value;

                    // Check the metadata of the entity and update its state
                    var root         = HierarchyUtils.GetRoot(targetEntity, Parents);
                    var activeStates = Metadata[root];

                    if (activeStates.Value.TryGetValue(targetEntity, out bool isActive)) {
                        isActive = !isActive;
                        activeStates.Value[targetEntity] = isActive;
                    }

                    if (isActive && Disabled.HasComponent(targetEntity) && 
                        (ShowButtons.HasComponent(msgEntity) || ToggleButtons.HasComponent(msgEntity))) {

                        CmdBuffer.RemoveComponent<Disabled>(targetEntity);
                        CmdBuffer.AddComponent<EnableRenderingTag>(targetEntity);

                        if (LocalVertices.HasComponent(targetEntity)) {
                            CmdBuffer.AddComponent<UpdateVertexColorTag>(targetEntity);
                        }
                        RecurseChildrenAndEnable(targetEntity, ref activeStates.Value);
                    }

                    if (!isActive && (CloseButtons.HasComponent(msgEntity) || ToggleButtons.HasComponent(msgEntity))) {
                        CmdBuffer.AddComponent<Disabled>(targetEntity);

                        if (LocalVertices.HasComponent(targetEntity)) {
                            CmdBuffer.AddComponent<UpdateVertexColorTag>(targetEntity);
                        }

                        RecurseChildrenAndDisabled(targetEntity);
                    }
                }
            }


            private void RecurseChildrenAndDisabled(Entity entity) {
                if (!Children.HasComponent(entity)) {
                    return;
                }

                var children = Children[entity].AsNativeArray();

                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    CmdBuffer.AddComponent<Disabled>(child);
                    CmdBuffer.AddComponent<UpdateVertexColorTag>(child);
                    RecurseChildrenAndDisabled(child);
                }
            }

            private void RecurseChildrenAndEnable(Entity entity, ref UnsafeHashMap<Entity, bool> metadata) {
                if (!Children.HasComponent(entity)) {
                    return;
                }

                var children = Children[entity].AsNativeArray();

                for (int i = 0; i < children.Length; i++) {
                    var child = children[i].Value;

                    CmdBuffer.AddComponent<EnableRenderingTag>(child);
                    CmdBuffer.AddComponent<UpdateVertexColorTag>(child);

                    metadata.TryGetValue(child, out bool isActive);

                    if (isActive) {
                        CmdBuffer.RemoveComponent<Disabled>(child);
                    }

                    RecurseChildrenAndEnable(child, ref metadata);
                }
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var job           = new ToggleJob {
                CmdBuffer     = cmdBufferSystem.CreateCommandBuffer(),
                Parents       = GetComponentDataFromEntity<Parent>(true),
                Metadata      = GetComponentDataFromEntity<ChildrenActiveMetadata>(false),
                Children      = GetBufferFromEntity<Child>(true),
                Disabled      = GetComponentDataFromEntity<Disabled>(true),
                ShowButtons   = GetComponentDataFromEntity<ShowButtonType>(true),
                CloseButtons  = GetComponentDataFromEntity<CloseButtonType>(true),
                ToggleButtons = GetComponentDataFromEntity<ToggleButtonType>(true),
                LocalVertices = GetBufferFromEntity<LocalVertexData>(true)
            };

            Dependency = Entities.WithAll<ButtonMessageRequest>().
                WithAny<ShowButtonType, CloseButtonType, ToggleButtonType>().
                ForEach((Entity entity, DynamicBuffer<CloseTarget> b0) => {

                job.Execute(entity, b0);
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
