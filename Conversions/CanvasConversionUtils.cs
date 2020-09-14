using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDOTS.Conversions.Systems {
    internal static class CanvasConversionUtils {
        internal static void CleanCanvas(Entity canvasEntity, EntityManager manager) {
            manager.RemoveComponent<Rotation>(canvasEntity);
            manager.RemoveComponent<Translation>(canvasEntity);
            manager.RemoveComponent<NonUniformScale>(canvasEntity);
            manager.RemoveComponent<Scale>(canvasEntity);
        }

        internal static void SetScaleMode(Entity entity, Canvas canvas, EntityManager manager) {
            var canvasScaler = canvas.GetComponent<CanvasScaler>();
            switch (canvasScaler.uiScaleMode) {
                case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                    manager.AddComponentData(entity, new ReferenceResolution {
                        Value = canvasScaler.referenceResolution
                    });

                    // TODO: Should figure out if I want to support shrinking and expanding only...
                    if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight) {
                        manager.AddComponentData(entity, new WidthHeightRatio {
                            Value =  canvasScaler.matchWidthOrHeight
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
