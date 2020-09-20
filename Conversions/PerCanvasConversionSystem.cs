using UGUIDOTS.Analyzers;
using UGUIDOTS.Transforms;
using UnityEngine;
using static UGUIDOTS.Analyzers.BakedCanvasData;

namespace UGUIDOTS.Conversions.Systems {

    internal class PerCanvasConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {
                var canvasEntity = GetPrimaryEntity(canvas);
                DstEntityManager.RemoveComponent<Anchor>(canvasEntity);

                if (canvas.TryGetComponent(out BakedCanvasRunner runner)) {
                    var bakedData = runner.BakedCanvasData;
                    var idx = runner.Index;

                    var hierarchy = bakedData.Transforms[idx];
                    var root = canvas.transform;

                    DstEntityManager.SetComponentData(canvasEntity, new LocalToWorldRect {
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

                DstEntityManager.SetComponentData(childEntity, new LocalToWorldRect {
                    Scale       = associatedXform.WScale,
                    Translation = associatedXform.WPosition
                });

                DstEntityManager.SetComponentData(childEntity, new LocalToParentRect {
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
