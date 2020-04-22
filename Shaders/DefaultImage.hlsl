#ifndef UGUIDOTS_UNLIT_TRANSLATION
#define UGUIDOTS_UNLIT_TRANSLATION

#include "Common.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float, _Fill);
    UNITY_DEFINE_INSTANCED_PROP(float, _FillType);
    UNITY_DEFINE_INSTANCED_PROP(float4, _Translation);
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST);
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor);
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial);

struct Attributes 
{
    float3 positionOS: POSITION;
    float4 color:      COLOR;
    float2 baseUV:     TEXCOORD0;
};

struct Varyings 
{
    float4 positionCS: SV_POSITION;
    float4 color:      COLOR;
    float2 baseUV:     VAR_BASE_UV;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, ouput);
    float3 positionWS = TransformObjectToWorld(input.positionOS + _Translation.xyz);
    output.positionCS = TransformWorldToHClip(positionWS);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    output.color  = input.color;

    return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base      = baseMap * input.color;

#if defined (_FILL)
    float fill = 1 - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Fill);
    float type = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FillType);

    float increment;

    if (type == 0) 
    {
        increment = step(input.baseUV.x, fill * baseMap.x);
    }

    if (type == 1) 
    {
        increment = step(1 - fill * baseMap.x, input.baseUV.x);
    }

    if (type == 2) 
    {
        increment = step(input.baseUV.y, fill * baseMap.y);
    }

    if (type == 3) 
    {
        increment = step(1 - fill * baseMap.y, input.baseUV.y);
    }

    clip(base.a - increment - 0.1);
#endif

    return base;
}

#endif
