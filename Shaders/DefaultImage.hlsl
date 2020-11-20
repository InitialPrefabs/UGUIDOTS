#ifndef UGUIDOTS_UNLIT_TRANSLATION
#define UGUIDOTS_UNLIT_TRANSLATION

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float,  _Axis);
    UNITY_DEFINE_INSTANCED_PROP(float,  _Angle)
    UNITY_DEFINE_INSTANCED_PROP(float,  _Arc1)
    UNITY_DEFINE_INSTANCED_PROP(float,  _Arc2)
    UNITY_DEFINE_INSTANCED_PROP(float,  _Fill)
    UNITY_DEFINE_INSTANCED_PROP(float,  _FillType)
    UNITY_DEFINE_INSTANCED_PROP(float,  _Flip)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes 
{
    half3 positionOS: POSITION;
    half4 color:      COLOR;
    half2 uv:         TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings 
{
    half4 positionCS: SV_POSITION;
    half4 color:      COLOR;
    half2 uv:         TEXCOORD0;

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

half4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    half4 baseMap   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 base      = baseMap * input.color;

    float axis = UNITY_ACCESS_INSTANCED_PROP(float, _Axis);

#if defined (_FILL)
    half fillType = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _FillType);
    int flip      = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Flip);
    half fill     = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Fill);

    if (fillType == 0)
    {
        AxisFill(axis == 0 ? input.uv.x : input.uv.y, fill, flip);
    } 
    else 
    {
        half angle = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Angle);
        half arc1  = (1.0 - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Arc1)) * 360.0;
        half arc2  = (1.0 - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Arc2)) * 360.0;

        RadialFill(angle, arc1, arc2, input.uv);
    }
#endif

    return base;
}

#endif
