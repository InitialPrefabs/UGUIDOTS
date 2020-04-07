using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    [UpdateInGroup(typeof(InputGroup))]
    public unsafe class MobileMouseCollisionSystem : SystemBase {

        protected unsafe override void OnUpdate() {

            var touches = new NativeArray<TouchElement>(10, Allocator.Temp);

            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                UnsafeUtility.MemCpy(touches.GetUnsafePtr(), b0.GetUnsafePtr(), 
                    UnsafeUtility.SizeOf<TouchElement>() * b0.Length);
            }).Run();

            Entities.WithDeallocateOnJobCompletion(touches).WithNone<ButtonDisabledTag>().
                ForEach((ref ClickState c0, ref ButtonVisual c1, in Dimensions c2, in LocalToWorld c3, in ButtonClickType c4) => {
                var aabb = new AABB {
                    Center  = c3.Position,
                    Extents = new float3(c2.Extents(), c3.Position.z)
                };

                for (int i = 0; i < touches.Length; i++) {
                    var touch = touches[i];
                    if (aabb.Contains(new float3(touches[i].Position, c3.Position.z))) {
                        switch (c4.Value) {
                            case var _ when ClickType.PressDown == c4.Value && touch.Phase == TouchPhase.Began:
                                c0.Value = true;
                                break;
                            case var _ when ClickType.ReleaseUp == c4.Value && touch.Phase == TouchPhase.Ended:
                                c0.Value = true;
                                break;
                            default:
                                break;
                        }

                        var onTop = (touch.Phase & (TouchPhase.Began | TouchPhase.Stationary | TouchPhase.Moved)) > 0;
                        c1.Value  = onTop ? ButtonVisualState.Pressed : ButtonVisualState.Hover;
                        break;
                    } else {
                        c1.Value  = ButtonVisualState.None;
                    }
                }
            }).Run();
        }
    }
}
