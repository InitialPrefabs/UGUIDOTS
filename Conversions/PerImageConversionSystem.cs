using System.Collections.Generic;
using UGUIDots.Render;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class DeclareRenderDataConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                DeclareReferencedAsset(texture);

                var material = image.material != null ? image.material : Canvas.GetDefaultCanvasMaterial();
                DeclareReferencedAsset(material);
            });
        }
    }

    [UpdateBefore(typeof(HierarchyConversionSystem))]
    public class PerImageConversionSystem : GameObjectConversionSystem {

        private Dictionary<int, Material> uniqueMaterials;

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                // Generate the associated texture and material entities
                var sprite = image.sprite;

                // TODO: Test out writing a shader that is a complete rewrite of unity's default UI shader.
                var material = image.material;
                material = material != null ? material : Canvas.GetDefaultCanvasMaterial();

                var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;

                var materialEntity = GetPrimaryEntity(material);
                var textureEntity = GetPrimaryEntity(texture);
                var imageEntity = GetPrimaryEntity(image);

                DstEntityManager.AddComponentObject(materialEntity, material);
                DstEntityManager.AddComponentObject(textureEntity, texture);

                DstEntityManager.AddComponentData(imageEntity, new LinkedMaterialEntity {
                    Value = materialEntity
                });

                DstEntityManager.AddComponentData(imageEntity, new LinkedTextureEntity { 
                    Value = textureEntity 
                });

                // Set some names to make things convenient...
                #if UNITY_EDITOR
                DstEntityManager.SetName(textureEntity, $"[Texture]: {texture.name}");
                DstEntityManager.SetName(materialEntity, $"[Material]: {material.name}");
                #endif
            });
        }
    }
}
