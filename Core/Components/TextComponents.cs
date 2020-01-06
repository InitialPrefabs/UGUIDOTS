using Unity.Collections;
using Unity.Entities;

namespace UGUIDots {

    public struct Text32 : IComponentData {
        public NativeString32 Value;
    }

    public struct Text64 : IComponentData {
        public NativeString64 Value;
    }

    public struct Text128 : IComponentData {
        public NativeString128 Value;
    }

    public struct Text512 : IComponentData {
        public NativeString512 Value;
    }

    public struct Text4096 : IComponentData {
        public NativeString512 Value;
    }
}
