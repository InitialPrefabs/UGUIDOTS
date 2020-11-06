using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class CursorCollisionSystem : SystemBase {

        protected override void OnUpdate() {
            if (!HasSingleton<CursorTag>()) {
#if UNITY_EDITOR
                Debug.LogError("No Entity with the type: CursorTag found!");
#endif
                return;
            }

            var cursorEntity = GetSingletonEntity<CursorTag>();
            var cursors = GetBuffer<Cursor>(cursorEntity).AsNativeArray();

            var screenSpaces = GetComponentDataFromEntity<ScreenSpace>(true);

            Entities.WithAll<ScreenSpace>().ForEach((
                Entity entity,
                ref ButtonMouseVisualState c0, 
                ref ButtonInvoked c1, 
                in  ButtonMouseClickType c2,
                in  Dimension c3,
                in RootCanvasReference c4) => {

                var screenSpace = screenSpaces[entity];
                var rootSpace = screenSpaces[c4.Value];

                var aabb    = new AABB {
                    Center  = new float3(screenSpace.Translation, 0),
                    Extents = new float3(c3.Extents() * rootSpace.Scale, 0)
                };

                var buttonState = (int)c2.Value;
                c1.Value = false;

                for (int i = 0; i < cursors.Length; i++) {
                    var cursor = cursors[i];
                    var position = new float3(cursor.Position, 0);

                    // TODO: Check if the cursor has been pressed
                    if (aabb.Contains(position)) {
                        c0.Value = cursor.State != ClickState.None ? ButtonVisualState.Pressed : ButtonVisualState.Hover;

                        var cursorState = (int)cursor.State;
                        if (buttonState == cursorState) {
                            c1.Value = true;
                        }
                    } else {
                        c0.Value = ButtonVisualState.Default;
                    }

                }
            }).WithReadOnly(cursors).Run();
        }
    }
}
