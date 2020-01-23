using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    /// <summary>
    /// Constructs the batches required to do rendering.
    /// </summary>
    [UpdateInGroup(typeof(MeshBatchingGroup))]
    [UpdateAfter(typeof(BuildImageVertexDataSystem))]
    public class RenderRecurseOrderSystem : ComponentSystem {

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

            renderBatchArchetype = EntityManager.CreateArchetype(new [] {
                ComponentType.ReadWrite<RenderGroupID>(),
                ComponentType.ReadWrite<RenderElement>(),
                ComponentType.ReadOnly<UnsortedRenderTag>()
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

                // TODO: Might need multiple queries?
                // This needs to be a bit flag instead, wonder if there's a way to quickly check if an entity belongs to
                // this particular archetype.
                var hasMaterial  = EntityManager.HasComponent<Material>(child);
                var hasVertices  = EntityManager.HasComponent<MeshVertexData>(child);
                var hasIndices   = EntityManager.HasComponent<TriangleIndexElement>(child);
                var hasTransform = EntityManager.HasComponent<LocalToWorld>(child);
                var hasMesh      = EntityManager.HasComponent<MeshIndex>(child);

                if (childBuffer.Exists(child)) {
                    var grandChildren = childBuffer[child];
                    RecurseChildren(in grandChildren, in childBuffer);
                }

                if (hasMaterial && hasIndices && hasVertices && hasTransform && hasMesh) {
                    batchedEntityList.Add(child);
                }
            }
        }
    }
}
