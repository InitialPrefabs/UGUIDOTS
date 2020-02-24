using Unity.Entities;

namespace UGUIDots {

    /// <summary>
    /// Defines if the state of the button based on the positions of the cursor(s).
    /// </summary>
    public enum ButtonState : byte {
        None    = 1 >> 0,
        Hover   = 1 >> 1,
        Pressed = 1 >> 2
    }

    /// <summary>
    /// Stores if the button has a cursor on top of it, or if the cursor has been pressed 
    /// </summary>
    public struct CursorState : IComponentData {
        public ButtonState Value;
    }
}
