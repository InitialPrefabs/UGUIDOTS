using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Controls.Authoring {

    public enum ButtonVisibilityType {
        Close,
        Show,
        Toggle
    }

    public class CloseButtonAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public ButtonVisibilityType Type = ButtonVisibilityType.Toggle;
        public GameObject[] Targets;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
#if UNITY_EDITOR
            if (Targets.Length == 0) {
                throw new System.InvalidOperationException("Cannot send a message to a GameObject that is null!");
            }
#endif

            // Create the message which will try to close something
            var msg = dstManager.CreateEntity();

            switch (Type) {
                case ButtonVisibilityType.Toggle:
                    dstManager.AddComponentData(msg, new ToggleButtonType { });
                    break;
                case ButtonVisibilityType.Show:
                    dstManager.AddComponentData(msg, new ShowButtonType { });
                    break;
                case ButtonVisibilityType.Close:
                    dstManager.AddComponentData(msg, new CloseButtonType { });
                    break;
                default:
                    break;
            }

            var buffer = dstManager.AddBuffer<CloseTarget>(msg); 

            foreach (var target in Targets) {
                buffer.Add(new CloseTarget { Value = conversionSystem.GetPrimaryEntity(target) });
            }

            dstManager.AddComponentData(entity, new ButtonMessageFramePayload { Value = msg });

#if UNITY_EDITOR
            dstManager.SetName(msg, "Close Target Msg");
#endif
        }
    }
}
