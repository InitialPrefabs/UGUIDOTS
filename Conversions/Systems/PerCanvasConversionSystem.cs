using System.Collections.Generic;
using UGUIDOTS.Analyzers;
using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

using static UGUIDOTS.Analyzers.BakedCanvasData;

namespace UGUIDOTS.Conversions.Systems {

    internal class PerCanvasConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            var canvasMap = new Dictionary<Entity, int2>();
            Entities.ForEach((Canvas canvas) => {
                var canvasEntity = GetPrimaryEntity(canvas);
                DstEntityManager.RemoveComponent<Anchor>(canvasEntity);

                var canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasMap.Add(canvasEntity, new int2(canvasScaler.referenceResolution));

                CanvasConversionUtils.CleanCanvas(canvasEntity, DstEntityManager);
                CanvasConversionUtils.SetScaleMode(canvasEntity, canvas, DstEntityManager, canvasScaler);

                if (canvas.TryGetComponent(out BakedCanvasDataProxy runner)) {
                    var bakedData = runner.BakedCanvasData;
                    var idx = runner.Index;

                    var hierarchy = bakedData.Transforms[idx];
                    var root = canvas.transform;

                    DstEntityManager.SetComponentData(canvasEntity, new ScreenSpace {
                        Scale       = hierarchy.WScale,
                        Translation = hierarchy.WPosition
                    });

                    RecurseChildren(root, hierarchy);
                }
            });
        }

        private void RecurseChildren(Transform parent, CanvasTransform parentData) {
            for (int i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);
                var associatedXform = parentData.Children[i];
                var childEntity = GetPrimaryEntity(child);

                DstEntityManager.SetComponentData(childEntity, new ScreenSpace {
                    Scale       = associatedXform.WScale,
                    Translation = associatedXform.WPosition
                });

                DstEntityManager.SetComponentData(childEntity, new LocalSpace {
                    Scale       = associatedXform.LScale,
                    Translation = associatedXform.LPosition
                });

                if (parent.childCount > 0) {
                    RecurseChildren(child, associatedXform);
                }
            }
        }
    }
}
