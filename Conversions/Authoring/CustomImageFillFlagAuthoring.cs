using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Render.Authoring {

    [RequireComponent(typeof(ImageConversion))]
    public class CustomImageFillFlagAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        [Range(0f, 1f)]
        public float Arc1 = 1f;

        [Range(0f, 1f)]
        public float Arc2 = 1f;

        [Range(0f, 360f)]
        public float AngleOffset;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new RadialFillAmount {
                Angle = AngleOffset,
                Arc1  = Arc1,
                Arc2  = Arc2
            });
        }
    }
}
