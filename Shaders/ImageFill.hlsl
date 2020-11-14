#ifndef UGUIDOTS_IMAGE_FILL
#define UGUIDOTS_IMAGE_FILL

/**
 * Fill amounts we want to consider are
 * 0: L -> R
 * 1: R -> L
 * 2: B -> T
 * 3: T -> B
 * Radial Fill
 * 4: Fill from the top counter clockwise: 90 degrees, Arc 1
 * 5: Fill from the top clockwise: 90 Degrees, Arc 2
 * 6: Fill from the bottom counter clockwise: 270, Arc 1
 * 7: Fill from the bottom clockwise: 270, Arc 2
 * 8: Fill from the left counter clockwise: 180: Arc 1
 * 9: Fill from the left clockwise: 180, Arc 2
 * 10: Fill from the right counter clockwise: 360 Arc 1
 * 11: Fill from the right clockwise: 360 Arc 2
 */

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

inline void RadialFill(float angle, float arc1, float arc2, float2 uv) 
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
        clip(uvCoord < fill ? -1 : 1);
    }
    else
    {
        clip(uvCoord > fill ? -1 : 1);
    }
}

#endif
