using System.Runtime.CompilerServices;
using UGUIDOTS.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Render {

    public static class ImageUtils {
   
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddImageIndices(
            this ref NativeList<RootTriangleIndexElement> indices,
            in NativeList<RootVertexData> vertices) {

            var nextStartIdx = indices.Length > 1 ? indices[indices.Length - 1] + 1 : 0;

            indices.Add(new RootTriangleIndexElement { Value = (ushort)(0 + nextStartIdx) });
            indices.Add(new RootTriangleIndexElement { Value = (ushort)(1 + nextStartIdx) });
            indices.Add(new RootTriangleIndexElement { Value = (ushort)(2 + nextStartIdx) });
            indices.Add(new RootTriangleIndexElement { Value = (ushort)(0 + nextStartIdx) });
            indices.Add(new RootTriangleIndexElement { Value = (ushort)(2 + nextStartIdx) });
            indices.Add(new RootTriangleIndexElement { Value = (ushort)(3 + nextStartIdx) });
        }
        
        // TODO: Move this to MeshUtils...
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddVertices(
            this ref NativeList<RootVertexData> buffer, 
            float4 position,
            SpriteData data, 
            Color32 color) {

            var normalColor = color.ToNormalizedFloat4();
            var uv2         = new float2(1);

            buffer.Add(new RootVertexData {
                Color    = normalColor,
                Normal   = new float3(0, 0, -1),
                Position = new float3(position.xy, 0),
                UV1      = data.OuterUV.xy,
                UV2      = uv2
            });

            buffer.Add(new RootVertexData {
                Color    = normalColor,
                Normal   = new float3(0, 0, -1),
                Position = new float3(position.xw, 0),
                UV1      = data.OuterUV.xw,
                UV2      = uv2
            });

            buffer.Add(new RootVertexData {
                Color    = normalColor,
                Normal   = new float3(0, 0, -1),
                Position = new float3(position.zw, 0),
                UV1      = data.OuterUV.zw,
                UV2      = uv2
            });

            buffer.Add(new RootVertexData {
                Color    = normalColor,
                Normal   = new float3(0, 0, -1),
                Position = new float3(position.zy, 0),
                UV1      = data.OuterUV.zy,
                UV2      = uv2
            });
        }
        
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
