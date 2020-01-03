using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(BuildMeshSystem))]
    public class RenderBatchSystem : ComponentSystem {

        private EntityArchetype renderBatchArchetype;
        private EntityQuery canvasQuery;
        private List<Entity> batchedEntityList;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<CanvasSortOrder>(),
                    ComponentType.ReadOnly<DirtyTag>(),
                    ComponentType.ReadOnly<Child>()
                }
            });

            renderBatchArchetype = EntityManager.CreateArchetype(new[] {
                ComponentType.ReadWrite<RenderGroupID>(),
                ComponentType.ReadWrite<RenderElement>()
            });

            batchedEntityList = new List<Entity>();

            RequireForUpdate(canvasQuery);
        }

        protected override void OnUpdate() {
            var childrenBuffer = GetBufferFromEntity<Child>(true);

            Entities.With(canvasQuery).ForEach((Entity entity, CanvasSortOrder s0, DynamicBuffer<Child> b0) => {
                // Clear the list so that we can build a render hierarchy.
                batchedEntityList.Clear();
                var renderBatchEntity = PostUpdateCommands.CreateEntity(renderBatchArchetype);

                PostUpdateCommands.SetComponent(renderBatchEntity, new RenderGroupID { Value = s0.Value });
                var buffer = PostUpdateCommands.AddBuffer<RenderElement>(renderBatchEntity);

                RecurseChildren(in b0, in childrenBuffer);

                buffer.ResizeUninitialized(batchedEntityList.Count);

                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] = new RenderElement { Value = batchedEntityList[i] };
                }

                PostUpdateCommands.RemoveComponent<DirtyTag>(entity);
            });
        }

        private void RecurseChildren(in DynamicBuffer<Child> children, in BufferFromEntity<Child> childBuffer) {
            for (int i = 0; i < children.Length; i++) {
                var child = children[i].Value;

                if (childBuffer.Exists(child)) {
                    var grandChildren = childBuffer[child];
                    RecurseChildren(in grandChildren, in childBuffer);
                }

                batchedEntityList.Add(child);
            }
        }
    }
}
