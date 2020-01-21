using System.Collections.Generic;
using UGUIDots.Render;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    // TODO: Have a versioning system such that entities reference the correct blobs if more blobs are created.
    /// <summary>
    /// Converts images by associating the material to the Image based chunk.
    /// </summary>
    public class ImageConversionSystem : GameObjectConversionSystem {

        private HashSet<int> visited;
        private List<Image> images;

        protected override void OnCreate() {
            base.OnCreate();
            images = new List<Image>();
            visited = new HashSet<int>();
        }

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                // Add the image initially if it doesn't already exist
                if (!images.Contains(image)) {
                    images.Add(image);
                }

                var hash = image.GetHashCode();
                if (visited.Contains(hash)) {
                    return;
                }

                visited.Add(hash);

                var entity   = GetPrimaryEntity(image);
                var material = image.material != null ? image.material : Canvas.GetDefaultCanvasMaterial();
                DstEntityManager.AddComponentObject(entity, material);
                DstEntityManager.AddSharedComponentData(entity, new MaterialID { Value = material.GetInstanceID() });

                DstEntityManager.AddComponentData(entity, new ImageKey       { Value = images.IndexOf(image) });
                DstEntityManager.AddComponentData(entity, new AppliedColor   { Value = image.color });
                DstEntityManager.AddComponentData(entity, new Dimensions     { Value = image.rectTransform.Int2Size() });
                // DstEntityManager.AddComponentData(entity, new MeshRebuildTag { });

                // TODO: Does not handle image slicing
                DstEntityManager.AddBuffer<MeshVertexData>(entity).ResizeUninitialized(4);
                DstEntityManager.AddBuffer<TriangleIndexElement>(entity).ResizeUninitialized(6);
            });

            if (images.Count > 0) {
                // TODO: Have a check because if the conversion system continuously runs, then we have multiple smaller
                // mini blobs with more images.
                // Create a mega blob which references all of the sprites/images that we need.
                var entity     = DstEntityManager.CreateEntity();
                var collection = new TextureCollectionBlob { BlobAsset = ConstructBlob() };

                DstEntityManager.AddComponentData(entity, collection);
#if UNITY_EDITOR
                DstEntityManager.SetName(entity, "UI Texture Atlas");
#endif
            }
        }

        private BlobAssetReference<TextureArrayPtr> ConstructBlob() {
            var size     = images.Count;
            var builder  = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<TextureArrayPtr>();
            var textures = builder.Allocate(ref root.Ptr, size);

            for (int i = 0; i < size; i++) {
                textures[i] = images[i].mainTexture;
            }

            var blobAsset = builder.CreateBlobAssetReference<TextureArrayPtr>(Allocator.Persistent);
            builder.Dispose();
            return blobAsset;
        }
    }
}
