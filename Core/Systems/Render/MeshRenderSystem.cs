using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

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
        private EntityQuery drawableQuery, renderQuery;
        private MaterialPropertyBlock block;

        protected override void OnCreate() {
            base.OnCreate();
            drawableQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<LocalToWorld>(), 
                    ComponentType.ReadOnly<ImageDimensions>(),
                    ComponentType.ReadOnly<RenderMaterial>()
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });

            renderQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<RenderCommand>() 
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

            Entities.WithoutBurst().WithStoreEntityQueryInField(ref drawableQuery)
                .ForEach((Entity e, RenderMaterial s0, ref LocalToWorld c0, ref ImageDimensions c1) => {

                    var mesh          = buildMeshSystem.MeshWith(c1);
                    var propertyBlock = buildMeshSystem.PropertyBlockOf(e);

                    renderFeature.Pass.InstructionQueue.Enqueue((mesh, s0.Value, c0.Value, propertyBlock));
                }).Run();
            return inputDeps; 
        }
    }
}
