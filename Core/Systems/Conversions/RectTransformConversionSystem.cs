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

                // Adding the anchors - which is taking the anchored position
                DstEntityManager.AddComponentData(entity, new Anchor {
                    Distance = transform.anchoredPosition,
                    State    = transform.ToAnchor()
                });
            });
        }
    }
}
