using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    [UpdateInGroup(typeof(InputGroup))]
    public unsafe class MobileMouseCollisionSystem : SystemBase {

        protected unsafe override void OnUpdate() {

            var touches = stackalloc TouchElement[1];
            var size    = stackalloc int[1];

            Entities.ForEach((DynamicBuffer<TouchElement> b0) => {
                touches = (TouchElement*)b0.GetUnsafePtr();
                size[0] = b0.Length;
            }).WithNativeDisableUnsafePtrRestriction(touches).WithNativeDisableUnsafePtrRestriction(size).Run();

            Entities.WithNativeDisableUnsafePtrRestriction(touches).WithNativeDisableUnsafePtrRestriction(size).
                WithNone<ButtonDisabledTag>().
                ForEach((ref ClickState c0, ref ButtonVisual c1, in Dimensions c2, in LocalToWorld c3, in ButtonClickType c4) => {
                var aabb = new AABB {
                    Center  = c3.Position,
                    Extents = new float3(c2.Extents(), c3.Position.z)
                };

                for (int i = 0; i < size[0]; i++) {
                    var touch = touches[i];
                    if (aabb.Contains(new float3(touches[i].Position, c3.Position.z))) {
                        switch (c4.Value) {
                            case var _ when ClickType.PressDown == c4.Value && touch.Phase == TouchPhase.Began:
                                c0.Value = true;
                                break;
                            case var _ when ClickType.ReleaseUp == c4.Value && touch.Phase == TouchPhase.Ended:
                                c0.Value = true;
                                break;
                            case var _ when ClickType.Held == c4.Value && touch.Phase == TouchPhase.Began ||
                                (touch.Phase & (TouchPhase.Moved | TouchPhase.Stationary)) > 0:
                                c0.Value = true;
                                break;
                            default:
                                c0.Value = false;
                                break;
                        }

                        var onTop = touch.Phase == TouchPhase.Began || 
                            (touch.Phase & (TouchPhase.Stationary | TouchPhase.Moved)) > 0;

                        c1.Value  = onTop ? ButtonVisualState.Pressed : ButtonVisualState.Hover;
                        break;
                    } else {
                        c0.Value = false;
                        c1.Value = ButtonVisualState.None;
                    }
                }
            }).Run();
        }
    }
}
