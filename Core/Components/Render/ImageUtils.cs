using System.Runtime.CompilerServices;
using UGUIDots.Transforms;
using Unity.Mathematics;

namespace UGUIDots.Render {

    public static class ImageUtils {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 BuildImageVertexData(
            DefaultSpriteResolution resolution, 
            SpriteData spriteData, 
            Dimensions dimension, 
            float4x4 matrix) {

            var position = matrix.c3.xyz;
            var scale    = matrix.Scale();
            var extents  = dimension.Extents();

            var spriteScale = dimension.Value / (float2)resolution.Value;

            var padding = spriteData.Padding;

            var pixelAdjustments = new float4(
                (padding.x * spriteScale.x) / dimension.Width(),
                (padding.y * spriteScale.y) / dimension.Height(),
                (dimension.Width() - padding.z * spriteScale.x) / dimension.Width(),
                (dimension.Height() - padding.w * spriteScale.y) / dimension.Height()
            );

            var pixelYAdjust = spriteScale.y * 1.5f;
            var topAdjust    = spriteScale.y * (padding.w > 0 ? 1f : 0f);
            var bottomAdjust = spriteScale.y * (padding.y > 0 ? 1f : 0f);
            var bottomLeft   = position.xy - dimension.Extents() * scale.xy;

            return new float4(
                bottomLeft.x + dimension.Width() * pixelAdjustments.x * scale.x,
                (bottomLeft.y + dimension.Height() * pixelAdjustments.y * scale.y) + bottomAdjust,
                bottomLeft.x + dimension.Width() * pixelAdjustments.z * scale.x,
                (bottomLeft.y + dimension.Height() * pixelAdjustments.w * scale.y) - topAdjust
            );
        }
    }
}
