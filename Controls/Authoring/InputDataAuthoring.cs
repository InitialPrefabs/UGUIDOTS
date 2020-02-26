using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    public class InputDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public KeyCode PrimaryMouseKey = KeyCode.Mouse0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new PrimaryMouseKeyCode { Value = PrimaryMouseKey });
        }
    }
}
