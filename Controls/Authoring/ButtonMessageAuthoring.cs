using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    [RequireComponent(typeof(ButtonTypeAuthoring))]
    public class ButtonMessageAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        protected ButtonTypeAuthoring buttonTypeAuthoring;

        private void Awake() {
            buttonTypeAuthoring = GetComponent<ButtonTypeAuthoring>();
        }

        protected virtual void GenerateFrameMessagingEntity() {
            throw new System.NotImplementedException();
        }

        protected virtual void GeneratePersistentMessagingEntity() {
            throw new System.NotImplementedException();
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            switch (buttonTypeAuthoring.Type) {
                case ClickType.Held:
                    GeneratePersistentMessagingEntity();
                    break;
                case ClickType.PressDown:
                    GenerateFrameMessagingEntity();
                    break;
                case ClickType.ReleaseUp:
                    GenerateFrameMessagingEntity();
                    break;
            }
        }
    }
}
