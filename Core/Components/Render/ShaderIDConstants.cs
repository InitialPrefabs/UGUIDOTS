using UnityEngine;

namespace UGUIDots.Render {

    public static class ShaderIDConstants {
        public static readonly int MainTex   = Shader.PropertyToID("_MainTex");
        public static readonly int ColorMask = Shader.PropertyToID("_ColorMask");
        public static readonly int Color     = Shader.PropertyToID("_Color");
    }
}
