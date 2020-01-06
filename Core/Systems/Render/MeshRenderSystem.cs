using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    /// <summary>
    /// Barebones MeshRenderSystem allows access to the BuildMeshSystem's internal cache of meshes required for
    /// rendering.
    /// </summary>
    [UpdateAfter(typeof(BuildMeshSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class RenderSystem : JobComponentSystem {
        protected BuildMeshSystem buildMeshSystem;

        protected override void OnCreate() {
            buildMeshSystem = World.GetOrCreateSystem<BuildMeshSystem>();
        }
    }

    public class MeshRenderSystem : RenderSystem {

        private OrthographicRenderFeature renderFeature;
        private EntityQuery drawableQuery, renderQuery, batchedRenderQuery;
        private RenderSortSystem renderSortSystem;

        protected override void OnCreate() {
            base.OnCreate();
            renderSortSystem  = World.GetOrCreateSystem<RenderSortSystem>();
            batchedRenderQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<RenderGroupID>(),
                    ComponentType.ReadOnly<RenderElement>()
                }
            });

            RequireSingletonForUpdate<RenderCommand>();
        }

        protected override void OnStartRunning() {
            Entities.WithStoreEntityQueryInField(ref renderQuery).WithoutBurst().ForEach((RenderCommand cmd) => {
                renderFeature = cmd.RenderFeature;
            }).Run();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // TODO: See if there's a better way of doing this...
            inputDeps.Complete();

            var keys          = GetComponentDataFromEntity<ImageKey>(true);
            var dimensions    = GetComponentDataFromEntity<Dimensions>(true);
            var renderBuffers = GetBufferFromEntity<RenderElement>(true);
            var localToWorlds = GetComponentDataFromEntity<LocalToWorld>(true);
            var pairs         = renderSortSystem.SortedOrderPairs;

            for (int i = 0; i < pairs.Count; i++) {
                var pair   = pairs[i];
                var buffer = renderBuffers[pair.Root];

                for (int k = 0; k < buffer.Length; k++) {
                    var current = buffer[k].Value;
                    var dim     = dimensions[current];

                    var propertyBlock = buildMeshSystem.PropertyBlockOf(current);
                    var mesh          = buildMeshSystem.MeshWith(dim);
                    var renderMat     = EntityManager.GetSharedComponentData<RenderMaterial>(current);
                    var ltw           = localToWorlds[current].Value;

                    renderFeature.Pass.InstructionQueue.Enqueue((mesh, renderMat.Value, ltw, propertyBlock));
                }
            }
            return inputDeps;
        }
    }
}
