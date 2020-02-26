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
            public bool Pressed;
        }

        protected override void OnUpdate() {
            var keycode  = GetSingleton<PrimaryMouseKeyCode>().Value;
            var mousePos = Input.mousePosition;
            var pressed  = Input.GetKey(keycode);

            var mouseInfo = new NativeArray<MouseInfo>(1, Allocator.TempJob);
            mouseInfo[0] = new MouseInfo { Position = mousePos, Pressed = pressed };

            Dependency = Entities.WithReadOnly(mouseInfo).
                ForEach((ref CursorState c0, in Dimensions c1, in LocalToWorld c2) => {
                var aabb = new AABB {
                    Center  = c2.Position,
                    Extents = new float3(c1.Extents(), c2.Position.z)
                };

                if (aabb.Contains(mouseInfo[0].Position)) {
                    c0.Value = mouseInfo[0].Pressed ? ButtonState.Pressed : ButtonState.Hover;
                } else {
                    c0 = new CursorState { Value = ButtonState.None };
                }
            }).WithDeallocateOnJobCompletion(mouseInfo).ScheduleParallel(Dependency);
        }
    }
}
