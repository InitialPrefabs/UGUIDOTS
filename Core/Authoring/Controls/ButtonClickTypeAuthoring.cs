using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS {

    public class ButtonClickTypeAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public ButtonClickType ButtonClickType;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new ButtonMouseClickType {
                Value = ButtonClickType
            });
        }
    }
}
