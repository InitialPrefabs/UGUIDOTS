Shader "UGUIDOTS/Unlit/ImageFillTest"
{
    Properties
    {
        [PerRendererData] 
        _MainTex ("Main Texture", 2D)        = "white" {}
        _BaseColor ("Color", Color)         = (1, 1, 1, 1)
        _Angle ("Angle", Range(0, 360))      = 0
        _Arc1 ("Arc Point 1", Range(0, 360)) = 15
        _Arc2 ("Arc Point 2", Range(0, 360)) = 15
        _CutOff ("Alpha Clipping", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Cull Back
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex UnlitVertexPass
            #pragma fragment UnlitFragmentPass
            #include "TestImageFill.hlsl"
            ENDHLSL
        }
    }
}
