Shader "UGUIDOTS/Unlit/DefaultImage"
{
    // TODO: Add a custom editor script for this.
    Properties
    {
        [PerRendererData] _MainTex("Texture", 2D) = "white" {}

        // Fill options
        // ---------------------------------------------------------------------
        [Toggle(_FILL)] _ToggleFill ("Toggle Fill", Float) = 1

        // Axis Fill
        // ---------------------------------------------------------------------
        [Enum(UGUIDOTS.FillType)] _FillType ("Fill Type", Float) = 0
        [Enum(UGUIDOTS.Axis)] _Axis ("Axis", Float) = 0

        _Fill("Fill", Range(0, 1)) = 1
        [Toggle] _Flip ("Flip Fill", Float) = 0

        // Radial Fill
        // ---------------------------------------------------------------------
        _Angle ("Angle", Range(0, 360))    = 0
        _Arc1 ("Arc Point 1", Range(0.0, 1.0)) = 0
        _Arc2 ("Arc Point 2", Range(0.0, 1.0)) = 0
        
        // Stencil Operation should follow this: https://docs.unity3d.com/ScriptReference/Rendering.StencilOp.html
        // Stencil Compare should follow this: https://docs.unity3d.com/ScriptReference/Rendering.CompareFunction.html
        // ---------------------------------------------------------------------
        [HideInInspector] [IntRange] _StencilComp ("Stencil Comparison", Range(0, 7))        = 0
        [HideInInspector] [IntRange] _Stencil ("Stencil Ref", Range(0, 255))                 = 0
        [HideInInspector] [IntRange] _StencilOp ("Stencil Operation", Range(0, 7))           = 0
        [HideInInspector] [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 255
        [HideInInspector] [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0, 255))   = 255
        [HideInInspector] _ColorMask ("Color Mask", Float)                                   = 15
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

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Blend     SrcAlpha OneMinusSrcAlpha
            ZWrite    Off
            ZTest     [unity_GUIZTestMode]
            ColorMask [_ColorMask]

            HLSLPROGRAM
            #pragma shader_feature _FILL
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment

            #include "Common.hlsl"
            #include "ImageFill.hlsl"
            #include "DefaultImage.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "UGUIDOTS.EditorTools.DefaultImageEditor"
}
