using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    public class StandaloneInputAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public KeyCode PrimaryMouseKey = KeyCode.Mouse0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new PrimaryMouseKeyCode { Value = PrimaryMouseKey });

            dstManager.RemoveComponent<LocalToWorld>(entity);
            dstManager.RemoveComponent<Translation>(entity);
            dstManager.RemoveComponent<Rotation>(entity);
            dstManager.RemoveComponent<Scale>(entity);
            dstManager.RemoveComponent<NonUniformScale>(entity);
        }
    }
}
