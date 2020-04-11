using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    [UpdateInGroup(typeof(InputGroup))]
    public class StandaloneMouseCollisionSystem : SystemBase {

        private struct MouseInfo {
            public float3 Position;
            public bool2 Pressed;
        }

        protected override void OnCreate() {
            RequireSingletonForUpdate<PrimaryMouseKeyCode>();
        }

        protected override void OnUpdate() {
            var keycode   = GetSingleton<PrimaryMouseKeyCode>().Value;
            var mousePos  = Input.mousePosition;
            var clickDown = Input.GetKeyDown(keycode);
            var clickUp   = Input.GetKeyUp(keycode);
            var clickHeld = Input.GetKey(keycode);

            var mouse = new MouseInfo {
                Position = mousePos,
                Pressed = new bool2(clickDown, clickUp)
            };

            Dependency = Entities.WithNone<ButtonDisabledTag>().ForEach((ref ClickState c0, ref ButtonVisual c1, 
                in Dimensions c2, in LocalToWorld c3, in ButtonClickType c4) => {

                var aabb = new AABB {
                    Center  = c3.Position,
                    Extents = new float3(c2.Extents(), c3.Position.z)
                };

                if (aabb.Contains(mouse.Position)) {
                    switch (c4.Value) {
                        case ClickType.PressDown:
                            c0.Value = clickDown;
                            break;
                        case ClickType.ReleaseUp:
                            c0.Value = clickUp;
                            break;
                        case ClickType.Held:
                            c0.Value = clickHeld;
                            break;
                        default:
                            break;
                    }

                    c1.Value =  clickHeld ? ButtonVisualState.Pressed : ButtonVisualState.Hover;
                } else {
                    c1.Value = ButtonVisualState.None;
                }
            }).ScheduleParallel(Dependency);
        }
    }
}
