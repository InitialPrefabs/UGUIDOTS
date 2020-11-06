using System.Runtime.CompilerServices;
using Unity.Entities;

namespace UGUIDOTS {

    public enum ButtonVisualState : byte {
        None    = 0,
        Hover   = 1 << 0,
        Pressed = 1 << 1,
    }

    /// <summary>
    /// Stores if the button has a cursor on top of it, or if the cursor has been pressed 
    /// </summary>
    public struct ClickState : IComponentData {
        public bool Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClickState Default() => new ClickState { Value = false };
    }

    /// <summary>
    /// Stores the visual state of the button.
    /// </summary>
    public struct ButtonVisual : IComponentData {
        public ButtonVisualState Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ButtonVisual Default() => new ButtonVisual { Value = ButtonVisualState.None };
    }

    /// <summary>
    /// Marks an entity to have a message archetype.
    /// </summary>
    public struct ButtonMessageRequest : IComponentData { }

    /// <summary>
    /// Marks that a button is non interactable.
    /// </summary>
    public struct NonInteractableButtontag : IComponentData { }

    /// <summary>
    /// The button is strictly a close button type.
    /// </summary>
    public struct CloseButtonType : IComponentData { }

    /// <summary>
    /// The button is strictly a show button type.
    /// </summary>
    public struct ShowButtonType : IComponentData { }

    /// <summary>
    /// The button is strictly a toggle button type.
    /// </summary>
    public struct ToggleButtonType : IComponentData { }
}
