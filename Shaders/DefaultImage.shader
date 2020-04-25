Shader "UGUIDOTS/Unlit/DefaultImage"
{
    Properties
    {
        [PerRendererData] _MainTex("Texture", 2D) = "white" {}
        _BaseColor("Color", color) = (1.0, 1.0, 1.0, 1.0)
        _Translation("Translation", Vector) = (0.0, 0.0, 0.0)

        [Toggle(_FILL)] _ToggleFill ("Fill?", Float) = 1
        _Fill("Fill", Range(0, 1)) = 1
        [Enum(UGUIDots.Render.FillType)] _FillType ("Fill Type", Float) = 0

        /* 
         * Stencil Operation should follow this: https://docs.unity3d.com/ScriptReference/Rendering.StencilOp.html
         * Stencil Compare should follow this: https://docs.unity3d.com/ScriptReference/Rendering.CompareFunction.html
         */
        [IntRange] _StencilComp ("Stencil Comparison", Range(0, 7)) = 0
        [IntRange] _Stencil ("Stencil Ref", Range(0, 255)) = 0
        [IntRange] _StencilOp ("Stencil Operation", Range(0, 7)) = 0
        [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 255
        [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
    }

    SubShader
    {
        Tags 
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass 
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [unity_GUIZTestMode]
            ColorMask [_ColorMask]

            HLSLPROGRAM
            #pragma shader_feature _FILL
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "DefaultImage.hlsl"
            ENDHLSL
        }
    }
}
