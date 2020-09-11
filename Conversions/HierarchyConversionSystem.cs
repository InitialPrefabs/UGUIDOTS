using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UGUIDots.Analyzers;
using Unity.Entities;
using UGUIDots.Render;
using Unity.Mathematics;
using UGUIDots.Transforms;

namespace UGUIDots.Conversions.Systems {

    public static class ImageConversionUtils {
        public static void SetImageType(Entity entity, Image image, EntityManager manager) {
            switch (image.type) {
                case Image.Type.Simple:
                    break;
                case Image.Type.Filled:
                    SetFill(image, entity, manager);
                    break;
                default:
                    throw new NotImplementedException($"Only Simple/Filled Image types are supported {image.name}!");
            }
        }

        private static void SetFill(Image image, Entity entity, EntityManager manager) {
            var fillMethod = image.fillMethod;
            switch (fillMethod) {
                case Image.FillMethod.Vertical:
                    if (image.fillOrigin == (int)Image.OriginVertical.Bottom) {
                        manager.AddComponentData(entity, new FillAmount {
                            Amount = image.fillAmount,
                            Type = FillType.BottomToTop,
                        });
                    }

                    if (image.fillOrigin == (int) Image.OriginVertical.Top) {
                        manager.AddComponentData(entity, new FillAmount {
                            Amount = image.fillAmount,
                            Type = FillType.TopToBottom,
                        });
                    }
                    break;
                case Image.FillMethod.Horizontal:
                    if (image.fillOrigin == (int)Image.OriginHorizontal.Left) {
                        manager.AddComponentData(entity, new FillAmount {
                            Amount = image.fillAmount,
                            Type = FillType.LeftToRight
                        });
                    }

                    if (image.fillOrigin == (int)Image.OriginHorizontal.Right) {
                        manager.AddComponentData(entity, new FillAmount {
                            Amount = image.fillAmount,
                            Type = FillType.RightToLeft,
                        });
                    }
                    break;
                default:
                    throw new System.NotSupportedException("Radial support is not implemented yet.");
            }
        }

    }

    public class HierarchyConversionSystem : GameObjectConversionSystem {

        private List<Dictionary<int, (Material, Texture)>> batches;

        protected override void OnCreate() {
            base.OnCreate();

            batches = new List<Dictionary<int, (Material, Texture)>>();
        }

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

                    DstEntityManager.AddComponentData(imgEntity, new DefaultSpriteResolution { Value = spriteResolution });

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
