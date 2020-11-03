using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIDOTS {

    public class ButtonClickTypeAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        [FormerlySerializedAs("ButtonClickType")]
        public ButtonClickType ClickRegistrationType = ButtonClickType.PressDown;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new ButtonMouseClickType {
                Value = ClickRegistrationType
            });
        }
    }
}
