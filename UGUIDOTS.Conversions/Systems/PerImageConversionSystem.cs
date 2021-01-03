using UGUIDOTS.Render;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDOTS.Conversions.Systems {

    internal class PerImageConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                // Generate the associated texture and material entities
                var sprite = image.sprite;

                // TODO: Test out writing a shader that is a complete rewrite of unity's default UI shader.
                var material = image.material;
                var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;

                var materialEntity = GetPrimaryEntity(material);
                var textureEntity  = GetPrimaryEntity(texture);
                var imgEntity      = GetPrimaryEntity(image);

                DstEntityManager.AddComponentData(materialEntity, new SharedMaterial { Value = material });
                DstEntityManager.AddComponentData(textureEntity, new SharedTexture   { Value = texture });

                DstEntityManager.AddComponentData(imgEntity, new LinkedMaterialEntity {
                    Value = materialEntity
                });

                DstEntityManager.AddComponentData(imgEntity, new LinkedTextureEntity { 
                    Value = textureEntity 
                });

#if UNITY_EDITOR
                // Set some names to make things convenient to view in the debugger
                DstEntityManager.SetName(textureEntity, $"[Texture]: {texture.name}");
                DstEntityManager.SetName(materialEntity, $"[Material]: {material.name}");
#endif

                var color = image.color;
                if (image.TryGetComponent(out Button button)) {
                    color = button.interactable ? button.colors.normalColor : button.colors.disabledColor;
                }

                DstEntityManager.AddComponentData(imgEntity, new AppliedColor { Value = color });
                ImageConversionUtils.SetImageType(imgEntity, image, DstEntityManager);

                // Set up the texture
                var rectSize = image.rectTransform.Int2Size();
                var spriteResolution = image.sprite != null ? 
                    new int2(image.sprite.texture.width, image.sprite.texture.height) :
                    rectSize;

                DstEntityManager.AddComponentData(imgEntity, new DefaultSpriteResolution { 
                    Value = spriteResolution 
                });

                // Set up the sprite data
                DstEntityManager.AddComponentData(imgEntity, SpriteData.FromSprite(image.sprite));
            });
        }
    }
}
