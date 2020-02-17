using UGUIDots.Collections.Runtime;
using UGUIDots.Render;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    // TODO: Have a versioning system such that entities reference the correct blobs if more blobs are created.
    /// <summary>
    /// Converts images by associating the material to the Image based chunk.
    /// </summary>
    public class ImageConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                // TODO: Find a way to handle loading bins that hold textures and materials instead of using
                // Resources.Load
                TextureBin.TryLoadBin("TextureBin", out Bin<Texture> textureBin);
                MaterialBin.TryLoadBin("MaterialBin", out var materialBin);

                var texture    = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                var imageIndex = textureBin.Add(texture);

                var entity   = GetPrimaryEntity(image);
                var material = image.material != null ? image.material : Canvas.GetDefaultCanvasMaterial();
                DstEntityManager.AddComponentObject(entity, material);

                var rectSize = image.rectTransform.Int2Size();

                DstEntityManager.AddComponentData(entity, new TextureKey   { Value = imageIndex });
                DstEntityManager.AddComponentData(entity, new AppliedColor { Value = image.color });
                DstEntityManager.AddComponentData(entity, new Dimensions   { Value = rectSize });

                var spriteTexture = image.sprite;
                var spriteRes = spriteTexture != null ? 
                    new int2(spriteTexture.texture.width, spriteTexture.texture.height) :
                    rectSize;

                DstEntityManager.AddComponentData(entity, new DefaultSpriteResolution { Value = spriteRes });

                var spriteData = SpriteData.FromSprite(image.sprite);
                DstEntityManager.AddComponentData(entity, spriteData);

                // TODO: Does not handle image slicing
                DstEntityManager.AddBuffer<MeshVertexData>(entity).ResizeUninitialized(4);
                DstEntityManager.AddBuffer<TriangleIndexElement>(entity).ResizeUninitialized(6);
            });
        }
    }
}
