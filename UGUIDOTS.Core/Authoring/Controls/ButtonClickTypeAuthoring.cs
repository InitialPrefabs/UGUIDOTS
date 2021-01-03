using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS {

    public class ButtonClickTypeAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public ButtonClickType ClickRegistrationType = ButtonClickType.PressDown;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new ButtonMouseClickType {
                Value = ClickRegistrationType
            });
        }
    }
}
