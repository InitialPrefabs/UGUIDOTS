using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    public class InputDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public KeyCode PrimaryMouseKey = KeyCode.Mouse0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
#if UNITY_EDITOR || UNITY_STANDALONE
            dstManager.AddComponentData(entity, new PrimaryMouseKeyCode { Value = PrimaryMouseKey });
#elif UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            var buffer      = dstManager.AddBuffer<TouchElement>(entity);
            buffer.Capacity = 10;
            for (int i = 0; i < 10; i++) {
                buffer.Add(TouchElement.Default());
            }
#endif

            dstManager.RemoveComponent<LocalToWorld>(entity);
            dstManager.RemoveComponent<Translation>(entity);
            dstManager.RemoveComponent<Rotation>(entity);
            dstManager.RemoveComponent<Scale>(entity);
            dstManager.RemoveComponent<NonUniformScale>(entity);
        }
    }
}
