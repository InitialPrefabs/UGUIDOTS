using Unity.Entities;

namespace UGUIDots.Controls {

    public enum ClickType : byte {
        PressDown = 1 << 0,
        ReleaseUp = 1 << 1,
        Held      = 1 << 2
    }

    public struct ButtonClickType : IComponentData {
        public ClickType Value;
    }
}
