using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    /// <summary>
    /// Copies data from the Input engine into a TouchElement buffer.
    /// </summary>
    [UpdateInGroup(typeof(InputGroup))]
    public class CopyTouchDataSystem : SystemBase {

        protected override void OnUpdate() {
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                var touchBuffer = b0.AsNativeArray();
                for (int i = 0; i < Input.touchCount; i++) {
                    touchBuffer[i] = Input.GetTouch(i);
                    // Debug.Log($"At {i}: {touchBuffer[i].Phase}");
                }

                for (int i = Input.touchCount; i < touchBuffer.Length; i++) {
                    touchBuffer[i] = TouchElement.Default();     
                }
            }).WithoutBurst().Run();
        }
    }
}
