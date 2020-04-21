using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AddMeshSystem : SystemBase {

        private EntityQuery canvasQuery;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<AddMeshTag>() },
                None = new [] { ComponentType.ReadOnly<Mesh>() }
            });

            RequireForUpdate(canvasQuery);
        }

        protected override void OnUpdate() {
            using (var entities = canvasQuery.ToEntityArray(Allocator.TempJob)) {
                foreach (var entity in entities) {
                    EntityManager.AddComponentObject(entity, new Mesh());
                    EntityManager.RemoveComponent<AddMeshTag>(entity);
                }
            }
        }
    }
}
