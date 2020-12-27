using System;
using UnityEngine.UI;
using Unity.Entities;
using UGUIDOTS.Render.Authoring;

namespace UGUIDOTS.Conversions.Systems {

    internal static class ImageConversionUtils {

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
                    manager.AddComponentData(entity, new AxisFillAmount {
                        Axis = Axis.Y,
                        FillAmount = image.fillAmount,
                        Flip = image.fillOrigin == (int)Image.OriginVertical.Top
                    });
                    break;
                case Image.FillMethod.Horizontal:
                    manager.AddComponentData(entity, new AxisFillAmount {
                        Axis       = Axis.X,
                        FillAmount = image.fillAmount,
                        Flip       = image.fillOrigin == (int)Image.OriginHorizontal.Right
                    });
                    break;
                case Image.FillMethod.Radial360:
                    if (!image.TryGetComponent(out CustomImageFillFlagAuthoring flag)) {
                        var originType = (Image.Origin360)image.fillOrigin;
                        manager.AddComponentData(entity, new RadialFillAmount {
                            Angle = DetermineAngleOffset(originType),
                            Arc1  = 1f,
                            Arc2  = image.fillAmount
                        });
                    }
                    break;
                default:
                    break;
            }
        }

        private static float DetermineAngleOffset(Image.Origin360 fillType) {
            switch (fillType) {
                case Image.Origin360.Bottom: // Arc 2
                    return 270f;
                case Image.Origin360.Right: // Arc 2
                    return 0f;
                case Image.Origin360.Left:  // Arc 2
                    return 180f;
                case Image.Origin360.Top: // Arc 2
                    return 90f;
                default:
                    return 0f;
            }
        }
    }
}
