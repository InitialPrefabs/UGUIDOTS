using Unity.Entities;

namespace UGUIDots.Transforms {

    [System.Flags]
    public enum StretchedState : byte {
        StretchXY = 1 >> 0

        // TODO: Support stretch along left, middle, right
        // TODO: Support stretch along top, middle, bottom
    }


    /// <summary>
    /// Represents that an element is stretched instead of anchored and scaled like the rest of the canvas system.
    /// </summary>
    public struct Stretch : IComponentData {
        public StretchedState Value;
    }
}
