using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS {
    
    /// <summary>
    /// Stores the state of clicking.
    /// </summary>
    public enum ClickState {
        None    = 0,
        Enter   = 1,
        Release = 2,
        Stay    = 3,
    }

    /// <summary>
    /// Marks a specific entity to be the entity which stores the cursors.
    /// </summary>
    public struct CursorTag : IComponentData { }

    /// <summary>
    /// Stores the cursor positions we use for comparisons.
    /// </summary>
    public struct Cursor : IBufferElementData {
        public float2 Position;
        public ClickState State;
    }
}
