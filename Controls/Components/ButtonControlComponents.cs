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
    [System.Obsolete]
    public struct ButtonArchetypeProducerRequest : IComponentData {
        public EntityArchetype Value;
    }

    /// <summary>
    /// Stores the entity that needs to be produced and consumed on the next frame.
    /// </summary>
    public struct ButtonMessageFramePayload : IComponentData {
        public Entity Value;
    }

    /// <summary>
    /// Stores the entity that needs to be produced and remain persistent until the button is released.
    /// </summary>
    public struct ButtonMessagePersistentPayload : ISystemStateComponentData {
        public Entity Value;
    }
}
