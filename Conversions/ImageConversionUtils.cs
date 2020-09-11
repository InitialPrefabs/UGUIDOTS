using System;
using UnityEngine.UI;
using Unity.Entities;
using UGUIDots.Render;

namespace UGUIDots.Conversions.Systems {

    internal static class ImageConversionUtils {
        internal static void GenerateAssociativeRenderData() {
        }

        internal static void SetImageType(Entity entity, Image image, EntityManager manager) {
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
}
