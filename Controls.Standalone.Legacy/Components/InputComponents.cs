using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Controls.Standalone {

    /// <summary>
    /// Stores the primary mouse key code.
    /// </summary>
    public struct PrimaryMouseKeyCode : IComponentData {
        public KeyCode Value;
    }
}
