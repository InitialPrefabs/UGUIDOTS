using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots {

    /*
    /// <summary>
    /// A representation of the LocalToWorld matrix.
    /// </summary>
    public struct LTW : IComponentData {
        public float4x4 Value;

        public static implicit operator Matrix4x4(LTW value) => (Matrix4x4)value.Value;
        public static implicit operator LTW(Matrix4x4 value) => new LTW { Value = value };

        public static implicit operator LTW(float4x4 value) => new LTW { Value = value };
        public static implicit operator float4x4(LTW value) => value.Value;
    }

    /// <summary>
    /// A presentation of the LocalToParent matrix.
    /// </summary>
    public struct LTP : IComponentData {
        public float4x4 Value;

        public static implicit operator Matrix4x4(LTP value) => (Matrix4x4)value.Value;
        public static implicit operator LTP(Matrix4x4 value) => new LTP { Value = value };
        public static implicit operator LTP(float4x4 value) => new LTP { Value = value };
        public static implicit operator float4x4(LTP value) => value.Value;
    }
    */
}
