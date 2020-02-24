using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Controls {

    public enum CursorState {
        Began,
        Moved,
        Ended
    }

    public struct PrimaryMouseKeyCode : IComponentData {
        public KeyCode Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PrimaryMouseKeyCode Default() {
            return new PrimaryMouseKeyCode { Value = KeyCode.Mouse0 };
        }
    }

    public struct CursorPositionElement : IBufferElementData {
        public float2 Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CursorPositionElement None() {
            return new CursorPositionElement { Value = new float2(float.PositiveInfinity, float.PositiveInfinity) };
        }
    }

    public struct CursorStateElement : IBufferElementData {
        public CursorState Value;
    }

    public static class InputExtension {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefault(this CursorPositionElement value) {
            return value.Value.Equals(new float2(float.PositiveInfinity, float.PositiveInfinity));
        }
    }
}
