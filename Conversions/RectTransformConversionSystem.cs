using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDOTS.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    internal class RectTransformConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((RectTransform transform) => {
                var entity = GetPrimaryEntity(transform);
                var rectSize = transform.Int2Size();
                DstEntityManager.AddComponentData(entity, new Dimensions { Value = rectSize });

                // Add anchoring if the min max anchors are equal (e.g. one of the presets)
                if (transform.anchorMin == transform.anchorMax) {

                    // Adding the anchors - which is taking the anchored position
                    DstEntityManager.AddComponentData(entity, new Anchor {
                        Distance = transform.anchoredPosition,
                        State    = transform.ToAnchor()
                    });
                } else {
                    DstEntityManager.AddComponentData(entity, new Stretch {
                        Value = StretchedState.StretchXY
                    });
                }

                DstEntityManager.AddComponentData(entity, new LocalToWorldRect { });

                if (transform.parent != null) {
                    DstEntityManager.AddComponentData(entity, new LocalToParentRect { });
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
