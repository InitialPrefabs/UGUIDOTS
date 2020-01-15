using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Conversions {

    public struct DynamicTag : IComponentData { }

    public class DynamicTagProxy : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        }
    }
}
