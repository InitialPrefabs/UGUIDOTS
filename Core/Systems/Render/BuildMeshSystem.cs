using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildMeshSystem : JobComponentSystem {

        // TODO: Add a reactive system such that if the dimensions change, then we rebuild the mesh by removing the tag.
        /// <summary>
        /// Internal tag so that meshes that have been cached haven't been rechecked.
        /// </summary>
        public struct CachedMeshTag : IComponentData { }

        private IDictionary<Dimensions, Mesh> meshCache;
        private IDictionary<Entity, MaterialPropertyBlock> propertyBlocks;
        private EntityQuery imgQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            meshCache      = new Dictionary<Dimensions, Mesh>();
            propertyBlocks = new Dictionary<Entity, MaterialPropertyBlock>();

            imgQuery = GetEntityQuery(new EntityQueryDesc { 
                All = new [] { 
                    ComponentType.ReadOnly<Dimensions>(),
                    ComponentType.ReadOnly<AppliedColor>()
                },
                Any = new [] {
                    ComponentType.ReadOnly<ImageKey>()
                },
                None = new [] {
                    ComponentType.ReadOnly<CachedMeshTag>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // TODO: Causes all jobs to sync - check if there's a way to defer grabbing the TextureCollectionBlob...
            var atlas      = GetSingleton<TextureCollectionBlob>();
            var imageKeys  = GetComponentDataFromEntity<ImageKey>(true);

            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            // TODO: Add a profiling marker
            Entities.WithStoreEntityQueryInField(ref imgQuery).WithoutBurst()
                .ForEach((Entity e, ref Dimensions c0, ref AppliedColor c2) => {

                if (!meshCache.ContainsKey(c0)) {
                    // Build the mesh
                    meshCache.Add(c0, MeshUtils.CreateQuad(c0.Width(), c0.Height()));
                }

                if (!propertyBlocks.ContainsKey(e)) {
                    var propertyBlock = new MaterialPropertyBlock {};

                    if (imageKeys.Exists(e)) {
                        propertyBlock.SetTexture(ShaderIDConstants.MainTex, atlas.At(imageKeys[e].Value));
                    }

                    propertyBlock.SetColor(ShaderIDConstants.Color, c2.Value);

                    // Build the property block that we can use
                    propertyBlocks.Add(e, propertyBlock);
                }

                cmdBuffer.AddComponent<CachedMeshTag>(e);
            }).Run();
            return inputDeps;
        }

        public Mesh MeshWith(Dimensions dim)                   => meshCache[dim];
        public MaterialPropertyBlock PropertyBlockOf(Entity e) => propertyBlocks[e];
    }
}
