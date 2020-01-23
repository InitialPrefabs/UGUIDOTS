using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    // TODO: Implement this
    [UpdateInGroup(typeof(MeshRenderGroup))]
    public class HybridMeshRenderSystem : JobComponentSystem {

        private MeshCacheSystem meshCacheSystem;
        private RenderSortSystem renderSortSystem;

        private EntityQuery orderedRenderQuery, renderCmdQuery, textureBinQuery;
        private OrthographicRenderFeature feature;

        private TextureBin textureBin;

        protected override void OnCreate() {
            meshCacheSystem = World.GetOrCreateSystem<MeshCacheSystem>();
            renderSortSystem = World.GetOrCreateSystem<RenderSortSystem>();

            orderedRenderQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<RenderGroupID>(),
                    ComponentType.ReadOnly<RenderElement>()
                }
            });

            renderCmdQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<RenderCommand>()
                }
            });

            textureBinQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<TextureBin>()
                }
            });
        }

        protected override void OnStartRunning() {
            Entities.WithStoreEntityQueryInField(ref renderCmdQuery).ForEach((RenderCommand cmd) => {
                feature = cmd.RenderFeature;
            }).WithoutBurst().Run();

            Entities.WithStoreEntityQueryInField(ref textureBinQuery).ForEach((TextureBin c0) => {
                textureBin = c0;
            }).WithoutBurst().Run();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            inputDeps.Complete();

            var keys          = GetComponentDataFromEntity<TextureKey>(true);
            var dimensions    = GetComponentDataFromEntity<Dimensions>(true);
            var localToWorlds = GetComponentDataFromEntity<LocalToWorld>(true);
            var renderBuffers = GetBufferFromEntity<RenderElement>(true);
            var meshIndices   = GetComponentDataFromEntity<MeshIndex>(true);
            var pairs         = renderSortSystem.SortedOrderPairs;

            for (int i = 0; i < pairs.Count; i++) {
                var pair    = pairs[i];
                var renders = renderBuffers[pair.Root];

                for (int k = 0; k < renders.Length; k++) {
                    var entity = renders[k].Value;
                    var meshIdx = meshIndices[entity].Value;
                    var hasMesh = meshCacheSystem.TryGetMeshPropertyAt(meshIdx, out var meshPropertyPair);

                    if (hasMesh) {
                        var ltw = localToWorlds[entity].Value;
                        var key = keys[entity].Value;

                        // TODO: I think a material idx would be ideal...
                        var material = EntityManager.GetComponentObject<Material>(entity);

                        var block = meshPropertyPair.PropertyBlock;
                        block.SetTexture(ShaderIDConstants.MainTex, textureBin.At(key));
                        feature.Pass.InstructionQueue.Enqueue((meshPropertyPair.Mesh, material, ltw, block));
                    }
                }
            }

            return inputDeps;
        }
    }
}
