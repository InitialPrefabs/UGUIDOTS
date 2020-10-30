using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS {

    public struct CursorTag : IComponentData { }

    /// <summary>
    /// Stores the cursor positions we use for comparisons.
    /// </summary>
    public struct Cursor : IBufferElementData {
        public struct Element {
            public float2 Position;
            public bool Pressed;
        }

        public Element Value;
    }
}
