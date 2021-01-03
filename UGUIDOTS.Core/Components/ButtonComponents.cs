using UGUIDOTS.Collections;
using Unity.Entities;

namespace UGUIDOTS {

    /// <summary>
    /// Defines how a button should be interacted.
    /// </summary>
    public enum ButtonClickType : byte {
        PressDown = 1,
        ReleaseUp = 2,
        Held      = 3
    }

    /// <summary>
    /// Defines the visual state of the button.
    /// </summary>
    public enum ButtonVisualState : byte {
        Default = 0,
        Hover   = 1 << 0,
        Pressed = 1 << 1,
    }

    /// <summary>
    /// Stores how the button behaves when clicked.
    /// </summary>
    public struct ButtonMouseClickType : IComponentData {
        public ButtonClickType Value;
    }

    /// <summary>
    /// Stores the visual state of the button.
    /// </summary>
    public struct ButtonMouseVisualState : IComponentData {
        public ButtonVisualState Value;
    }

    /// <summary>
    /// Stores whether the button was invoked.
    /// </summary>
    public struct ButtonInvoked : IComponentData {
        public bool Value;
    }

    /// <summary>
    /// Marks that a button is non interactable.
    /// </summary>
    public struct NonInteractableButtonTag : IComponentData { }

    /// <summary>
    /// Stores the entities which should be targetted for some kind of post processing.
    /// </summary>
    public struct TargetEntity : IBufferElementData, IStruct<TargetEntity> {
        public Entity Value;
    }
}
