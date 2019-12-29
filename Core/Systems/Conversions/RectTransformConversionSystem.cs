using Unity.Mathematics;
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
                DstEntityManager.AddComponentData(entity, new NonUniformScale { Value = transform.localScale });

                // TODO: Add the relative distances to the anchors
            });
        }
    }
}
