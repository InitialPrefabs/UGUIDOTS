using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Controls {

    /// <summary>
    /// Stores the primary mouse key code.
    /// </summary>
    public struct PrimaryMouseKeyCode : IComponentData {
        public KeyCode Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PrimaryMouseKeyCode Default() {
            return new PrimaryMouseKeyCode { Value = KeyCode.Mouse0 };
        }
    }

    /// <summary>
    /// Stores the position of the touch or mouse position.
    /// </summary>
    public struct CursorPositionElement : IBufferElementData {
        public float2 Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CursorPositionElement None() {
            return new CursorPositionElement { Value = new float2(float.PositiveInfinity, float.PositiveInfinity) };
        }
    }

    /// <summary>
    /// Stores whether the mouse or touch state has been pressed or held.
    /// </summary>
    public struct CursorStateElement : IBufferElementData {
        public bool Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CursorStateElement Default() {
            return new CursorStateElement { Value = false };
        }
    }

    public static class InputExtension {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefault(this CursorPositionElement value) {
            return value.Value.Equals(new float2(float.PositiveInfinity, float.PositiveInfinity));
        }
    }
}
