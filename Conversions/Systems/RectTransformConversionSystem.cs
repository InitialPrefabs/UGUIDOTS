using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDOTS.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    internal class RectTransformConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((RectTransform transform) => {
                var entity = GetPrimaryEntity(transform);

                // Add anchoring if the min max anchors are equal (e.g. one of the presets)
                if (transform.anchorMin == transform.anchorMax) {

                    // Adding the anchors - which is taking the anchored position
                    DstEntityManager.AddComponentData(entity, new Anchor {
                        Offset = transform.anchoredPosition,
                        State  = transform.ToAnchor()
                    });

                    var rectSize = transform.Int2Size();
                    DstEntityManager.AddComponentData(entity, new Dimension { Value = rectSize });
                } else {
                    DstEntityManager.AddComponentData(entity, new Stretch {
                        Value = StretchedState.StretchXY
                    });

                    var res = transform.root.GetComponent<CanvasScaler>().referenceResolution;
                    var rectSize = new int2((int)res.x, (int)res.y);
                    DstEntityManager.AddComponentData(entity, new Dimension { Value = rectSize });
                }

                DstEntityManager.AddComponentData(entity, new ScreenSpace { });

                if (transform.parent != null) {
                    DstEntityManager.AddComponentData(entity, new LocalSpace { });
                }

                // Remove all Unity Transforms
                DstEntityManager.RemoveComponent<Rotation>(entity);
                DstEntityManager.RemoveComponent<Translation>(entity);
                DstEntityManager.RemoveComponent<NonUniformScale>(entity);
                DstEntityManager.RemoveComponent<Scale>(entity);
                DstEntityManager.RemoveComponent<LocalToWorld>(entity);
                DstEntityManager.RemoveComponent<LocalToParent>(entity);
            });
        }
    }
}
