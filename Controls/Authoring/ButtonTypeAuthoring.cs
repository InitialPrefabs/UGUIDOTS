using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {
    public class ButtonTypeAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public ClickType Type = ClickType.ReleaseUp;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new ButtonClickType { Value = Type });
        }
    }
}
