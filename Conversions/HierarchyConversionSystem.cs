using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UGUIDots.Analyzers;
using Unity.Mathematics;
using UGUIDots.Transforms;

namespace UGUIDots.Conversions.Systems {

    internal class HierarchyConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {
                if (canvas.transform.parent != null) {
                    Debug.LogError("Cannot convert a canvas that is not a root canvas!");
                    return;
                }

                var batches = BatchAnalysis.BuildStaticBatch(canvas);

                foreach (var element in batches) {
                    BuildPerElement(element);
                }

                var canvasEntity = GetPrimaryEntity(canvas);
                CanvasConversionUtils.CleanCanvas(canvasEntity, DstEntityManager);
                CanvasConversionUtils.SetScaleMode(canvasEntity, canvas, DstEntityManager);
            });

        }

        private void BuildPerElement(List<GameObject> batch) {
            foreach (var gameObject in batch) {
                if (gameObject.TryGetComponent(out Image image)) {
                    var imgEntity = GetPrimaryEntity(image);

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

                    // Set up the sprite
                    DstEntityManager.AddComponentData(imgEntity, SpriteData.FromSprite(image.sprite));
                }

                if (gameObject.TryGetComponent(out TMP_Text txt)) {
                    var txtEntity = GetPrimaryEntity(txt);
                    DstEntityManager.AddComponentData(txtEntity, new AppliedColor { Value = txt.color });
                    DstEntityManager.AddComponentData(txtEntity, new TextOptions {
                        Size      = (ushort)txt.fontSize,
                        Style     = txt.fontStyle,
                        Alignment = txt.alignment.FromTextAnchor()
                    });
                }
            }
        }
    }
}
