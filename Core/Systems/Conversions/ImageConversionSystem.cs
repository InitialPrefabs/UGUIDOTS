using UGUIDots.Render;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    /// <summary>
    /// Converts images by associating the material to the Image based chunk.
    /// </summary>
    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    [UpdateAfter(typeof(VisualAssetsConversionSystem))]
    public class ImageConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                var texture  = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                var material = image.material != null ? image.material : Canvas.GetDefaultCanvasMaterial();

                var entity   = GetPrimaryEntity(image);
                var rectSize = image.rectTransform.Int2Size();

                DstEntityManager.AddComponentData(entity, new LinkedTextureEntity { Value = GetPrimaryEntity(texture) });
                DstEntityManager.AddComponentData(entity, new LinkedMaterialEntity { Value = GetPrimaryEntity(material) });
                DstEntityManager.AddComponentData(entity, new AppliedColor { Value = image.color });

                var spriteTexture = image.sprite;
                var spriteRes = spriteTexture != null ?
                    new int2(spriteTexture.texture.width, spriteTexture.texture.height) :
                    rectSize;

                DstEntityManager.AddComponentData(entity, new DefaultSpriteResolution { Value = spriteRes });

                var spriteData = SpriteData.FromSprite(image.sprite);
                DstEntityManager.AddComponentData(entity, spriteData);

                // TODO: Does not handle image slicing
                DstEntityManager.AddBuffer<LocalVertexData>(entity).ResizeUninitialized(4);
                DstEntityManager.AddBuffer<LocalTriangleIndexElement>(entity).ResizeUninitialized(6);

                // Mark that the image has to be built.
                DstEntityManager.AddComponent<BuildUIElementTag>(entity);
            });
        }
    }
}
