#ifndef UGUIDOTS_IMAGE_FILL
#define UGUIDOTS_IMAGE_FILL

inline float4 PixelSnap(float4 pos)
{
    float2 hpc = _ScreenParams.xy * 0.5f;
#if  SHADER_API_PSSL
// An old sdk used to implement round() as floor(x+0.5) current sdks use the round to even method so we manually use the old method here for compatabilty.
    float2 temp = ((pos.xy / pos.w) * hpc) + float2(0.5f,0.5f);
    float2 pixelPos = float2(floor(temp.x), floor(temp.y));
#else
    float2 pixelPos = round ((pos.xy / pos.w) * hpc);
#endif
    pos.xy = pixelPos / hpc * pos.w;
    return pos;
}

bool IsRadialDiscard(float angle, float arc1, float arc2, float2 uv) 
{
    float startAngle = angle - arc1;
    float endAngle   = angle + arc2;

    float offset0   = clamp(0, 360, startAngle + 360);
    float offset360 = clamp(0, 360, endAngle - 360);

    float2 atan2Coord = float2(lerp(-1, 1, uv.x), lerp(-1, 1, uv.y));
    float atanAngle   = atan2(atan2Coord.y, atan2Coord.x) * 57.3;

    if (atanAngle < 0) 
    {
        atanAngle += 360;
    }

    return atanAngle <= offset360 || atanAngle >= offset0 || atanAngle >= startAngle && atanAngle <= endAngle;
}

#endif
