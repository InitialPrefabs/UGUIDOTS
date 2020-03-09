using Unity.Entities;

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
    /// Stores the Archetype that the button will produce when clicked.
    /// </summary>
    public struct ButtonArchetypeProducerRequest : IComponentData {
        public EntityArchetype Value;
    }
}
