using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UGUIDOTS.Conversions.Systems {
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    internal class DeclareRenderDataConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Image image) => {
                var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                DeclareReferencedAsset(texture);

                var material = image.material != null ? image.material : Canvas.GetDefaultCanvasMaterial();
                DeclareReferencedAsset(material);
            });

            Entities.ForEach((TextMeshProUGUI text) => {
                // Declare the referenced font asset and materials per text
                DeclareReferencedAsset(text.font);
                DeclareReferencedAsset(text.materialForRendering);
            });
        }
    }
}
