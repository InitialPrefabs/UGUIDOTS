#ifndef UGUIDOTS_IMAGE_FILL
#define UGUIDOTS_IMAGE_FILL

inline half4 PixelSnap(half4 pos)
{
    half2 hpc = _ScreenParams.xy * 0.5f;
#if  SHADER_API_PSSL
    // An old sdk used to implement round() as floor(x+0.5) current sdks 
    // use the round to even method so we manually use the old method here for compatabilty.
    half2 temp = ((pos.xy / pos.w) * hpc) + float2(0.5f,0.5f);
    half2 pixelPos = float2(floor(temp.x), floor(temp.y));
#else
    half2 pixelPos = round ((pos.xy / pos.w) * hpc);
#endif
    pos.xy = pixelPos / hpc * pos.w;
    return pos;
}

inline void RadialFill(half angle, half arc1, half arc2, half2 uv) 
{
    half startAngle = angle - arc1;
    half endAngle   = angle + arc2;

    half offset0   = clamp(0, 360, startAngle + 360);
    half offset360 = clamp(0, 360, endAngle - 360);

    half2 atan2Coord = float2(lerp(-1, 1, uv.x), lerp(-1, 1, uv.y));
    half atanAngle   = atan2(atan2Coord.y, atan2Coord.x) * 57.3;

    if (atanAngle < 0) 
    {
        atanAngle += 360;
    }

    half clipped = atanAngle <= offset360 || atanAngle >= offset0 || atanAngle >= startAngle && atanAngle <= endAngle;
    clip(clipped ? -1 : 1);
}

inline void AxisFill(half uvCoord, half fill, int flip) 
{
    if (flip == 0) 
    {
        clip(uvCoord > fill ? -1 : 1);
    }
    else
    {
        clip(1 - fill > uvCoord ? -1 : 1);
    }
}

#endif
