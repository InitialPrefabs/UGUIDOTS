using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls {

    public class MobileInputAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public int Capacity = 10;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var buffer = dstManager.AddBuffer<TouchElement>(entity);
            buffer.Capacity = Capacity;
            for (int i = 0; i < Capacity; i++) {
                buffer.Add(TouchElement.Default());
            }
        }
    }
}
