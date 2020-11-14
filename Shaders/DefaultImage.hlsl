#ifndef UGUIDOTS_UNLIT_TRANSLATION
#define UGUIDOTS_UNLIT_TRANSLATION

#include "Common.hlsl"
#include "ImageFill.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float, _Axis);
    UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
    UNITY_DEFINE_INSTANCED_PROP(float, _FillType)
    UNITY_DEFINE_INSTANCED_PROP(float, _Flip)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes 
{
    float3 positionOS: POSITION;
    float4 color:      COLOR;
    float2 uv:         TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings 
{
    float4 positionCS: SV_POSITION;
    float4 color:      COLOR;
    float2 uv:         TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    output.uv     = input.uv * baseST.xy + baseST.zw;
    output.color  = input.color;

    return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 base      = baseMap * input.color;

    float axis = UNITY_ACCESS_INSTANCED_PROP(float, _Axis);

#if defined (_FILL)
    float fillType = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FillType);
    int flip       = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Flip);
    float fill     = 1 - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Fill);

    if (fillType == 0)
    {
        AxisFill(axis == 0 ? input.uv.x : input.uv.y, fill, flip);
    } 
    else 
    {
        // TODO: Do radial fill
    }
#endif

    return base;
}

#endif
