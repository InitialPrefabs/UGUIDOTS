using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Controls.Systems {

    /// <summary>
    /// Copies data from the Input engine into a TouchElement buffer.
    /// </summary>
    [UpdateInGroup(typeof(InputGroup))]
    public class CopyTouchDataSystem : SystemBase {

        protected override void OnUpdate() {
            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                var touchBuffer = b0.AsNativeArray();

                var size = math.min(Input.touchCount, touchBuffer.Length);

                for (int i = 0; i < size; i++) {
                    touchBuffer[i] = Input.GetTouch(i);
                }

                for (int i = size; i < touchBuffer.Length; i++) {
                    touchBuffer[i] = TouchElement.Default();
                }
            }).WithoutBurst().Run();
        }
    }
}
