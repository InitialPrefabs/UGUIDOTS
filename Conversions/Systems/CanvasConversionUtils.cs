using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace UGUIDOTS.Conversions.Systems {

    internal static class CanvasConversionUtils {
        internal static void CleanCanvas(Entity canvasEntity, EntityManager manager) {
            manager.RemoveComponent<Rotation>(canvasEntity);
            manager.RemoveComponent<Translation>(canvasEntity);
            manager.RemoveComponent<NonUniformScale>(canvasEntity);
            manager.RemoveComponent<Scale>(canvasEntity);
        }

        internal static void SetScaleMode(Entity entity, Canvas canvas, EntityManager manager, CanvasScaler canvasScaler) {
            switch (canvasScaler.uiScaleMode) {
                case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                    if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight) {
                        manager.AddComponentData(entity, new ReferenceResolution {
                            Value = canvasScaler.referenceResolution,
                            WidthHeightWeight = canvasScaler.matchWidthOrHeight
                        });

                        manager.AddComponentData(entity, new Dimension {
                            Value = new int2(canvasScaler.referenceResolution)
                        });
                    } else {
                        throw new NotSupportedException($"{canvasScaler.screenMatchMode} is not supported yet.");
                    }
                    break;
                default:
                    throw new NotSupportedException($"{canvasScaler.uiScaleMode} is not supported yet.");
            }
        }
    }
}
