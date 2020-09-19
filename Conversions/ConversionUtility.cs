using TMPro;
using UGUIDOTS.Render;
using UGUIDOTS.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Conversions {

    public static class ConversionUtility {
        public unsafe static void ConvertText(
            GameObjectConversionSystem conversion, 
            Entity canvasEntity,
            TextMeshProUGUI text, 
            ref NativeList<RootVertexData> vertexData, 
            ref NativeList<RootTriangleIndexElement> indexData) {

            // TODO: Generate an entity with all the glyphs stored as a hash map.
            var textEntity = conversion.GetPrimaryEntity(text);
            var textFontEntity = conversion.GetPrimaryEntity(text.font);
            var glyphTable = text.font.characterLookupTable;
            var glyphList = new NativeList<GlyphElement>(glyphTable.Count, Allocator.Temp);

            var glyphBuffer = conversion.DstEntityManager.GetBuffer<GlyphElement>(textFontEntity);
            var glyphData = new NativeArray<GlyphElement>(glyphBuffer.Length, Allocator.Temp);
            UnsafeUtility.MemCpy(glyphData.GetUnsafePtr(), glyphBuffer.GetUnsafePtr(),
                UnsafeUtility.SizeOf<GlyphElement>() * glyphData.Length);

            var fontFace = conversion.DstEntityManager.GetComponentData<FontFaceInfo>(textFontEntity);
            var textOption = conversion.DstEntityManager.GetComponentData<TextOptions>(textEntity);

            var charBuffer = conversion.DstEntityManager.GetBuffer<CharElement>(textEntity);
            var textBuffer = new NativeArray<CharElement>(charBuffer.Length, Allocator.Temp);
            UnsafeUtility.MemCpy(textBuffer.GetUnsafePtr(), charBuffer.GetUnsafePtr(),
                UnsafeUtility.SizeOf<CharElement>() * charBuffer.Length);

            var ltw = conversion.DstEntityManager.GetComponentData<LocalToWorldRect>(textEntity);
            var fontScale = textOption.Size > 0 ? textOption.Size / fontFace.PointSize : 1f;

            var dimension = conversion.DstEntityManager.GetComponentData<Dimensions>(textEntity);
            var extents = conversion.DstEntityManager.GetComponentData<Dimensions>(textEntity).Extents() * ltw.Scale.y;

            var lines = new NativeList<TextUtil.LineInfo>(Allocator.Temp);
            var isBold = textOption.Style == FontStyles.Bold;
            var styleSpaceMultiplier =
                1f + (isBold ? fontFace.BoldStyle.y * 0.01f : fontFace.NormalStyle.y * 0.01f);
            var padding = fontScale * styleSpaceMultiplier;

            TextUtil.CountLines(textBuffer, glyphData, dimension, padding, ref lines);

            var totalLineHeight = lines.Length * fontFace.LineHeight * fontScale * ltw.Scale.y;
            var heights = new float3(fontFace.LineHeight, fontFace.AscentLine, fontFace.DescentLine) *
                                    ltw.Scale.y;
            var stylePadding = TextUtil.SelectStylePadding(textOption, fontFace);

            var start = new float2(
                TextUtil.GetHorizontalAlignment(textOption.Alignment, extents, lines[0].LineWidth * ltw.Scale.x),
                TextUtil.GetVerticalAlignment(heights, fontScale, textOption.Alignment, extents,
                    totalLineHeight, lines.Length)) + ltw.Translation;

            for (int k = 0, row = 0; k < textBuffer.Length; k++) {
                var c = textBuffer[k].Value;

                if (!glyphData.TryGetGlyph(c, out var glyph)) {
                    continue;
                }

                var bl = (ushort)vertexData.Length;
                if (row < lines.Length && k == lines[row].StartIndex) {
                    var height = fontFace.LineHeight * fontScale * ltw.Scale.y * (row > 0 ? 1f : 0f);

                    start.y -= height;
                    start.x = TextUtil.GetHorizontalAlignment(textOption.Alignment,
                        extents, lines[row].LineWidth * ltw.Scale.x) + ltw.Translation.x;
                    row++;
                }

                var xPos = start.x + (glyph.Bearings.x - stylePadding) * fontScale * ltw.Scale.x;
                var yPos = start.y - (glyph.Size.y - glyph.Bearings.y - stylePadding) * fontScale * ltw.Scale.y;
                var size = (glyph.Size + new float2(stylePadding * 2)) * fontScale * ltw.Scale;
                var uv1 = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);

                var canvasScale = conversion.DstEntityManager.GetComponentData<LocalToWorldRect>(canvasEntity).Scale;
                var uv2 = new float2(glyph.Scale) * math.@select(canvasScale, -canvasScale, isBold);

                var right = new float3(1, 0, 0);
                var color = ((Color32)text.color).ToNormalizedFloat4();

                vertexData.Add(new RootVertexData {
                    Position = new float3(xPos, yPos, 0),
                    Normal = right,
                    Color = color,
                    UV1 = uv1.c0,
                    UV2 = uv2
                });

                vertexData.Add(new RootVertexData {
                    Position = new float3(xPos, yPos + size.y, 0),
                    Normal = right,
                    Color = color,
                    UV1 = uv1.c1,
                    UV2 = uv2
                });

                vertexData.Add(new LocalVertexData {
                    Position = new float3(xPos + size.x, yPos + size.y, 0),
                    Normal = right,
                    Color = color,
                    UV1 = uv1.c2,
                    UV2 = uv2
                });
                vertexData.Add(new LocalVertexData {
                    Position = new float3(xPos + size.x, yPos, 0),
                    Normal = right,
                    Color = color,
                    UV1 = uv1.c3,
                    UV2 = uv2
                });

                var tl = (ushort)(bl + 1);
                var tr = (ushort)(bl + 2);
                var br = (ushort)(bl + 3);

                indexData.Add(new RootTriangleIndexElement { Value = bl }); // 0
                indexData.Add(new RootTriangleIndexElement { Value = tl }); // 1
                indexData.Add(new RootTriangleIndexElement { Value = tr }); // 2

                indexData.Add(new RootTriangleIndexElement { Value = bl }); // 0
                indexData.Add(new RootTriangleIndexElement { Value = tr }); // 2
                indexData.Add(new RootTriangleIndexElement { Value = br }); // 3

                start += new float2(glyph.Advance * padding, 0) * ltw.Scale;
            }
        }
    }
}