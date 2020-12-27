using UnityEngine;

namespace UGUIDOTS.Render {

    public static class ShaderIDConstants {
        public static readonly int MainTex   = Shader.PropertyToID("_MainTex");
        public static readonly int ColorMask = Shader.PropertyToID("_ColorMask");
        public static readonly int Color     = Shader.PropertyToID("_Color");

        // Fill type
        // --------------------------------------------------------------------
        public static readonly int FillType = Shader.PropertyToID("_FillType");

        // Axis Fill
        // --------------------------------------------------------------------
        public static readonly int Fill = Shader.PropertyToID("_Fill");
        public static readonly int Axis = Shader.PropertyToID("_Axis");
        public static readonly int Flip = Shader.PropertyToID("_Flip");

        // Radial Fill
        // --------------------------------------------------------------------
        public static readonly int Arc1  = Shader.PropertyToID("_Arc1");
        public static readonly int Arc2  = Shader.PropertyToID("_Arc2");
        public static readonly int Angle = Shader.PropertyToID("_Angle");
    }
}
