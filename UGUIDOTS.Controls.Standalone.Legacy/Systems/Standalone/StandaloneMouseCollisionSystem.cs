using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Controls.Standalone.Systems {

    public unsafe class StandaloneMouseCollisionSystem : SystemBase {

        protected override void OnUpdate() {
            Vector2 mousePosition = Input.mousePosition;
            Entities.WithAll<CursorTag>().ForEach((DynamicBuffer<Cursor> b0, in PrimaryMouseKeyCode c0) => {
                var stay    = Input.GetKeyDown(c0.Value);
                var release = Input.GetKeyDown(c0.Value);

                Cursor* ptr = (Cursor*)b0.GetUnsafePtr();
                ptr->Position = mousePosition;

                switch (ptr->State) {
                    case ClickState.None:
                        if (Input.GetKeyDown(c0.Value)) {
                            ptr->State = ClickState.Enter;
                        }
                        break;
                    case ClickState.Release:
                        ptr->State = ClickState.None;
                        break;
                    case ClickState.Enter:
                        if (Input.GetKey(c0.Value)) {
                            ptr->State = ClickState.Stay;
                        }
                        break;
                    case ClickState.Stay:
                        if (Input.GetKeyUp(c0.Value)) {
                            ptr->State = ClickState.Release;
                        }
                        break;
                }
            }).WithoutBurst().Run();
        }
    }
}
