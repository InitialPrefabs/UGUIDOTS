using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(BuildMeshSystem))]
    public unsafe class RenderOrderSystem : ComponentSystem {

        // TODO: Initialize the native multi hash map with some capacity
        public NativeMultiHashMap<int, Entity> BatchedRenderOrder { get; private set; }

        private EntityQuery canvasQuery, sortOrderBufferQuery;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<CanvasSortOrder>(),
                    ComponentType.ReadOnly<DirtyTag>(),
                    ComponentType.ReadOnly<Child>()
                }
            });

            sortOrderBufferQuery = GetEntityQuery(new EntityQueryDesc { 
                All = new [] {
                    ComponentType.ReadOnly<SortOrderElement>() 
                }
            });

            RequireForUpdate(canvasQuery);
        }

        protected override void OnUpdate() {
            if (sortOrderBufferQuery.CalculateEntityCount() > 1) {
                return;
            }

            var sortOrderBuffer = EntityManager.GetBuffer<SortOrderElement>(sortOrderBufferQuery.GetSingletonEntity());

            for (int i = 0; i < sortOrderBuffer.Length; i++)
            {
                var filter = sortOrderBuffer[i].Value;
                canvasQuery.SetSharedComponentFilter(new CanvasSortOrder { Value = filter });
                var buffer = GetBufferFromEntity<Child>();

                Entities.With(canvasQuery).ForEach((Entity e, CanvasSortOrder order) => {
                    var children = buffer[e];
                    // RecurseChildren(in children);

                    PostUpdateCommands.RemoveComponent<DirtyTag>(e);
                });
            }
        }

        private void RecurseChildren(in DynamicBuffer<Child> children) {
            throw new System.NotImplementedException();
        }
    }
}
