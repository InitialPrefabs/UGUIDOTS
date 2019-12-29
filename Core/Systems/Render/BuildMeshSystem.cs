using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildMeshSystem : JobComponentSystem {

        private IDictionary<ImageDimensions, Mesh> meshCache;
        private IDictionary<Entity, MaterialPropertyBlock> propertyBlocks;
        private EntityQuery imgQuery, atlasQuery;

        protected override void OnCreate() {
            meshCache      = new Dictionary<ImageDimensions, Mesh>();
            propertyBlocks = new Dictionary<Entity, MaterialPropertyBlock>();

            // TODO: Add an exclusive component and only make the system run
            imgQuery = GetEntityQuery(new EntityQueryDesc { 
                All = new [] { 
                    ComponentType.ReadOnly<ImageDimensions>(),
                    ComponentType.ReadOnly<DefaultImageColor>()
                },
            });

            atlasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<TextureCollectionBlob>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var atlas = GetSingleton<TextureCollectionBlob>();

            // TODO: Add a profiling marker
            Entities.WithStoreEntityQueryInField(ref imgQuery).WithoutBurst()
                .ForEach((Entity e, ref ImageDimensions c0, ref DefaultImageColor c1) => {

                if (!meshCache.ContainsKey(c0)) {
                    // Build the mesh
                    meshCache.Add(c0, MeshUtils.CreateQuad(c0.Width(), c0.Height()));
                }

                if (!propertyBlocks.ContainsKey(e)) {
                    var propertyBlock = new MaterialPropertyBlock {};

                    propertyBlock.SetTexture(ShaderIDConstants.MainTex, atlas.At(c0.TextureKey));
                    propertyBlock.SetColor(ShaderIDConstants.ColorMask, c1.Value);

                    // Build the property block that we can use
                    propertyBlocks.Add(e, propertyBlock);
                }
            }).Run();
            return inputDeps;
        }

        public Mesh MeshWith(ImageDimensions dim) => meshCache[dim];
        public MaterialPropertyBlock PropertyBlockOf(Entity e) => propertyBlocks[e];
    }
}
