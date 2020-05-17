using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {
    public class CloseButtonAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public GameObject Target;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            // Create the message which will try to close something
            var msg = dstManager.CreateEntity();

            dstManager.AddComponentData(msg, new CloseTarget {
                Value = conversionSystem.GetPrimaryEntity(Target)
            });

            dstManager.AddComponentData(entity, new ButtonMessageFramePayload { Value = msg });
        }
    }
}
