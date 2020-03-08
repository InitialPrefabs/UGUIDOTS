using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    [UpdateInGroup(typeof(InputGroup))]
    public class CheckMouseCollisionSystem : SystemBase {

        private struct MouseInfo {
            public float3 Position;
            public bool2 Pressed;
        }

        protected override void OnStartRunning() {
        }

        protected override void OnUpdate() {
            var keycode   = GetSingleton<PrimaryMouseKeyCode>().Value;
            var mousePos  = Input.mousePosition;
            var clickDown = Input.GetKeyDown(keycode);
            var clickUp   = Input.GetKeyUp(keycode);
            var clickHeld = Input.GetKey(keycode);

            var mouseInfo = new NativeArray<MouseInfo>(1, Allocator.TempJob);
            mouseInfo[0]  = new MouseInfo { Position = mousePos, Pressed = new bool2(clickDown, clickUp) };

            Dependency = Entities.WithReadOnly(mouseInfo).
                ForEach((ref ClickState c0, in Dimensions c1, in LocalToWorld c2, in ButtonClickType c3) => {
                var aabb = new AABB {
                    Center  = c2.Position,
                    Extents = new float3(c1.Extents(), c2.Position.z)
                };

                if (aabb.Contains(mouseInfo[0].Position)) {
                } else {
                }
            }).WithDeallocateOnJobCompletion(mouseInfo).ScheduleParallel(Dependency);
        }
    }
}
