using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    public class InputDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var buffer = dstManager.AddBuffer<CursorPositionElement>(entity);

#if UNITY_EDITOR || UNITY_STANDALONE
            buffer.Capacity = 1;
            buffer.Add(CursorPositionElement.None());
#elif UNITY_ANDROID || UNITY_IOS
            // TODO: Add the touch buffer effectively
#endif
        }
    }
}
