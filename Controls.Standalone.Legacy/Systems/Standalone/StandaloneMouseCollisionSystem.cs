using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Controls.Standalone.Systems {

    public class StandaloneMouseCollisionSystem : SystemBase {

        protected override void OnUpdate() {
            Entities.WithAll<CursorTag>().ForEach((ref DynamicBuffer<Cursor> b0, in PrimaryMouseKeyCode c0) => {
                var clicked = Input.GetKeyDown(c0.Value);
                b0[0] = new Cursor {
                    Position = (Vector2)Input.mousePosition,
                    Pressed = clicked
                };
            }).WithoutBurst().Run();
        }
    }
}
