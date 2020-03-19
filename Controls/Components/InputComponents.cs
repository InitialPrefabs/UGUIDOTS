using System.Runtime.CompilerServices;
using Unity.Entities;
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
}
