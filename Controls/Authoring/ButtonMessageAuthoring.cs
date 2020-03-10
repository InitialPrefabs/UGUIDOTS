using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    public struct SampleValue : IComponentData {
        public int Value;
    }

    public class ButtonMessageAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new ButtonArchetypeProducerRequest { 
                Value = dstManager.CreateArchetype(ComponentType.ReadOnly<SampleValue>(), ComponentType.ReadOnly<ButtonMessageRequest>())
            });
        }
    }
}
