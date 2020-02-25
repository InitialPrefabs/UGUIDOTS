using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    // TODO: Implement whether a mouse is above an element
    [UpdateInGroup(typeof(InputGroup))]
    [UpdateAfter(typeof(UpdateMouseStateSystem))]
    public unsafe class CheckMouseCollisionSystem : SystemBase {

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInfo {
            public float2 Position;
            public bool Pressed;
        }

        [BurstCompile]
        private unsafe struct FetchMousePositionJob : IJobForEach_BB<CursorPositionElement, CursorStateElement> {

            [WriteOnly, NativeDisableUnsafePtrRestriction]
            public MouseInfo* MouseInfo;

            public void Execute([ReadOnly] DynamicBuffer<CursorPositionElement> b0, [ReadOnly] DynamicBuffer<CursorStateElement> b1) {
                Debug.Log($"Pressed: {b1[0].Value}");
                *MouseInfo = new MouseInfo {
                    Position = b0[0].Value,
                    Pressed = b1[0].Value
                };
            }
        }

        [BurstCompile]
        private unsafe struct TransitionButtonStateJob : IJobForEach<Dimensions, LocalToWorld, CursorState> {

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public MouseInfo* MouseInfo;

            public void Execute([ReadOnly] ref Dimensions c0, [ReadOnly] ref LocalToWorld c1, 
                ref CursorState c3) {

                var aabb = new AABB { Center = c1.Position, Extents = new float3(c0.Extents(), 0) };
                if (aabb.Contains(new float3(MouseInfo->Position, 0))) {
                    if (MouseInfo->Pressed) {
                        Debug.Log($"Pressed");
                        c3.Value = ButtonState.Pressed;
                    } else {
                        Debug.Log($"Hovering");
                        c3.Value = ButtonState.Hover;
                    }
                } else {
                    c3.Value = ButtonState.None;
                }
            }
        }

        private MouseInfo* mouseState;

        protected override void OnCreate() {
            mouseState = (MouseInfo*)(UnsafeUtility.Malloc(UnsafeUtility.SizeOf<MouseInfo>(), UnsafeUtility.AlignOf<MouseInfo>(), 
                Allocator.Persistent));

            *mouseState = default;
        }

        protected override void OnDestroy() {
            if (mouseState != null) {
                UnsafeUtility.Free(mouseState, Allocator.Persistent);
            }
        }

        protected override void OnUpdate() {
            *mouseState = default;

            Dependency = new FetchMousePositionJob {
                MouseInfo = mouseState
            }.Schedule(this, Dependency);

            Dependency = new TransitionButtonStateJob {
                MouseInfo = mouseState
            }.Schedule(this, Dependency);
        }
    }
}
