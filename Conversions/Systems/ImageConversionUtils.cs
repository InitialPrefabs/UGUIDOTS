using System;
using UnityEngine.UI;
using Unity.Entities;
using UGUIDOTS.Render;

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
                default:
                    // TODO: Implement radial fill
                    break;
            }
        }

    }
}
