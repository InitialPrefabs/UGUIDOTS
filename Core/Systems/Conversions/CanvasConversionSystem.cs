using System;
using System.Collections.Generic;
using UGUIDots.Transforms;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateAfter(typeof(RectTransformConversionSystem))]
    public class CanvasConversionSystem : GameObjectConversionSystem {

        private List<int> sortOrders = new List<int>();

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {

                var parent = canvas.transform.parent;
                if (parent != null) {
#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(canvas);
#endif
                    throw new NotSupportedException($"{canvas.name} is child of {parent.name}, this is not supported!");
                }

                var entity       = GetPrimaryEntity(canvas);
                var canvasScaler = canvas.GetComponent<CanvasScaler>();

                DstEntityManager.RemoveComponent<Anchor>(entity);
                DstEntityManager.AddSharedComponentData(entity, new CanvasSortOrder { Value = canvas.sortingOrder });
                DstEntityManager.AddComponentData(entity, new DirtyTag { });

                if (!sortOrders.Contains(canvas.sortingOrder)) {
                    sortOrders.Add(canvas.sortingOrder);
                }

                switch (canvasScaler.uiScaleMode) {
                    case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                        DstEntityManager.AddComponentData(entity, new ReferenceResolution {
                            Value = canvasScaler.referenceResolution
                        });

                        // TODO: Should figure out if I want to support shrinking and expanding only...
                        if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight) {
                            DstEntityManager.AddComponentData(entity, new WidthHeightRatio {
                                Value =  canvasScaler.matchWidthOrHeight
                            });
                        } else {
                            throw new NotSupportedException($"{canvasScaler.screenMatchMode} is not supported!");
                        }
                        break;
                    default:
                        throw new NotSupportedException($"{canvasScaler.uiScaleMode} is not supported!");
                }
            });
        }
    }
}
