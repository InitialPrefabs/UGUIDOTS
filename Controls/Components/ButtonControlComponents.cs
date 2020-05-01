using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Controls {

    /// <summary>
    /// The type of the button behaviour.
    /// </summary>
    public enum ClickType : byte {
        PressDown = 1 << 0,
        ReleaseUp = 1 << 1,
        Held      = 1 << 2
    }

    /// <summary>
    /// Registers how a button is clicked
    /// </summary>
    public struct ButtonClickType : IComponentData {
        public ClickType Value;
    }

    /// <summary>
    /// Stores the entity that needs to be produced and consumed on the next frame.
    /// </summary>
    public struct ButtonMessageFramePayload : IComponentData {
        public Entity Value;
    }

    /// <summary>
    /// Stores the touch state element recorded by Unity.
    /// </summary>
    public struct TouchElement : IBufferElementData {
        public TouchPhase Phase;
        public float2     Position;
        public short      TapCount;

        public static implicit operator TouchElement(Touch value) {
            return new TouchElement {
                Phase    = value.phase,
                Position = value.position,
                TapCount = (short)value.tapCount
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TouchElement Default() {
            return new TouchElement {
                Phase    = TouchPhase.Canceled,
                Position = new float2(float.PositiveInfinity, float.PositiveInfinity),
                TapCount = -1
            };
        }
    }
}
