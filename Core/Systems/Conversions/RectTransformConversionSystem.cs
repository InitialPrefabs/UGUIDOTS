using UGUIDots.Transforms;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Conversions.Systems {

    /// <summary>
    /// Converts all RectTransforms into its entities counterpart using LocalToWorld as its Matrix4x4 representation
    /// in ScreenSpace.
    /// </summary>
    public class RectTransformConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((RectTransform transform) => {
                var entity = GetPrimaryEntity(transform);

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
            });
        }
    }
}
