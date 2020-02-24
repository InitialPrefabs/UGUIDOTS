using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Authoring {

    public class InputDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public KeyCode PrimaryMouseKey = KeyCode.Mouse0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var posBuffer   = dstManager.AddBuffer<CursorPositionElement>(entity);
            posBuffer.ResizeUninitialized(1);
            posBuffer[0] = CursorPositionElement.None();

            var clickBuffer = dstManager.AddBuffer<CursorStateElement>(entity);
            clickBuffer.ResizeUninitialized(1);
            clickBuffer[0] = CursorStateElement.Default();

            dstManager.AddComponentData(entity, new PrimaryMouseKeyCode { Value = PrimaryMouseKey });
        }
    }
}
