using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

    /// <summary>
    /// Stores the intended color to apply to the entity.
    /// </summary>
    public struct AppliedColor : IComponentData {
        public Color32 Value;
    }

    /// <summary>
    /// Stores the key to the texture that needs to be displayed.
    /// </summary>
    public struct ImageKey : IComponentData {
        public int Value;
    }

}
