using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Render.Authoring {

    [System.Obsolete]
    public class MaterialPropertyAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        }
    }
}
