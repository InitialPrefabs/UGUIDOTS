using TMPro;
using UGUIDOTS.Render;
using UGUIDOTS.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDOTS.Conversions.Systems {

    internal class PerTextConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI text) => {
                var material = text.materialForRendering;

                if (material == null) {
                    Debug.LogError("A material is missing from the TextMeshUGUI component, conversion will not work!");
                    return;
                }

                var materialEntity = GetPrimaryEntity(material);
                DstEntityManager.AddComponentData(materialEntity, new SharedMaterial { Value = material });

#if UNITY_EDITOR
                DstEntityManager.SetName(materialEntity, $"[Material]: {material.name}");
#endif

                var textEntity = GetPrimaryEntity(text);
                DstEntityManager.AddComponentData(textEntity, new LinkedMaterialEntity { Value = materialEntity });
                DstEntityManager.AddComponentData(textEntity, new AppliedColor { Value = text.color });
                DstEntityManager.AddComponentData(textEntity, new TextOptions {
                    Size      = (ushort)text.fontSize,
                    Style     = text.fontStyle,
                    Alignment = text.alignment.FromTextAnchor()
                });

                AddTextData(textEntity, text.text);
            });
        }

        private unsafe void AddTextData(Entity e, string text) {
            var length = text.Length;

            var txtBuffer = DstEntityManager.AddBuffer<CharElement>(e);
            txtBuffer.ResizeUninitialized(length);

            fixed (char* start = text) {
                UnsafeUtility.MemCpy(txtBuffer.GetUnsafePtr(), start, UnsafeUtility.SizeOf<char>() * length);
            }
        }
    }

    internal class PerImageConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                // Generate the associated texture and material entities
                var sprite = image.sprite;

                // TODO: Test out writing a shader that is a complete rewrite of unity's default UI shader.
                var material = image.material;
                material = material != null ? material : Canvas.GetDefaultCanvasMaterial();

                var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;

                var materialEntity = GetPrimaryEntity(material);
                var textureEntity  = GetPrimaryEntity(texture);
                var imgEntity      = GetPrimaryEntity(image);

                DstEntityManager.AddComponentData(materialEntity, new SharedMaterial { Value =  material });
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

                DstEntityManager.AddComponentData(imgEntity, new AppliedColor { Value = image.color });
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
