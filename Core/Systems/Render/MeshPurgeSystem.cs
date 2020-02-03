using Unity.Entities;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchingGroup))]
    [UpdateBefore(typeof(MeshCacheSystem))]
    public class MeshPurgeSystem : ComponentSystem {

        private EntityQuery purgedQuery;
        private MeshCacheSystem meshCacheSystem;

        protected override void OnCreate() {
            purgedQuery = GetEntityQuery(new EntityQueryDesc {
                None = new [] { 
                    ComponentType.ReadOnly<Dimensions>(), ComponentType.ReadOnly<MeshVertexData>(),
                    ComponentType.ReadOnly<TriangleIndexElement>()
                },
                All = new [] { 
                    ComponentType.ReadOnly<MeshIndex>()
                }
            });

            meshCacheSystem = World.GetOrCreateSystem<MeshCacheSystem>();

            RequireForUpdate(purgedQuery);
        }

        protected override void OnUpdate() {
            Entities.With(purgedQuery).ForEach((Entity e, ref MeshIndex c0) => {
                meshCacheSystem.Purge(c0.Value);
                PostUpdateCommands.RemoveComponent<MeshIndex>(e);
            });
        }
    }
}
