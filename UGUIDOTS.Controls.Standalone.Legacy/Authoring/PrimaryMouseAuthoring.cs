using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Controls.Standalone {

    public class PrimaryMouseAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
        
        public KeyCode Mouse0 = KeyCode.Mouse0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new PrimaryMouseKeyCode { Value = Mouse0 });
        }
    }
}
