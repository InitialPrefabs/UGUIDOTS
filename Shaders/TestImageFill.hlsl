#ifndef TEST_IMAGE_FILL
#define TEST_IMAGE_FILL

#include "Common.hlsl"
#include "ImageFill.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float _CutOff;

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float, _Angle)
    UNITY_DEFINE_INSTANCED_PROP(float, _Arc1)
    UNITY_DEFINE_INSTANCED_PROP(float, _Arc2)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor);
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST);
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
    float2 uv:         VAR_BASE_UV;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitVertexPass(Attributes input) 
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    output.uv    = input.uv * baseST.xy + baseST.zw;
    output.color = input.color;

    return output;
}

float4 UnlitFragmentPass(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base      = baseMap * input.color;

    float angle = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Angle);
    float arc1  = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Arc1);
    float arc2  = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Arc2);

    RadialFill(angle, arc1, arc2, input.uv);
    // clip(isDiscarded ? -1 : base.a - _CutOff);
    return base;
}

#endif
