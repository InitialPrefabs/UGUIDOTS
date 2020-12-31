using System.Runtime.CompilerServices;
using TMPro;
using UGUIDOTS.Collections;
using UGUIDOTS.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDOTS.Render.Systems {

    internal static class TextBuildUtility {
        internal static void CopyToBuffer(
            DynamicBuffer<Vertex> dstVertices, 
            DynamicBuffer<Index> dstIndices, 
            NativeList<Vertex> srcVertices, 
            NativeList<Index> srcIndices,
            int2 origStaticSpan) {

            var vertexLength = math.min(dstVertices.Length - origStaticSpan.x, srcVertices.Length);

            for (int i = 0; i < vertexLength; i++) {
                dstVertices[i + origStaticSpan.x] = srcVertices[i];
            }

            for (int i = vertexLength; i < srcVertices.Length; i++) {
                dstVertices.Add(srcVertices[i]);
            }

            var indexLength = math.min(dstIndices.Length - origStaticSpan.y, srcIndices.Length);

            for (int i = 0; i < indexLength; i++) {
                dstIndices[i + origStaticSpan.y] = srcIndices[i];
            }

            for (int i = indexLength; i < srcIndices.Length; i++) {
                dstIndices.Add(srcIndices[i]);
            }
        }

        internal static bool FindGlyphWithChar(NativeArray<GlyphElement> glyphs, char c, out GlyphElement glyph) {
            var unicode = (ushort)c;
            for (int i = 0; i < glyphs.Length; i++) {
                var current = glyphs[i];
                if (current.Unicode == unicode) {
                    glyph = current;
                    return true;
                }
            }

            glyph = default;
            return false;
        }

        internal static void CreateVertexForChars(
            Entity canvasEntity,
            Entity textEntity,
            NativeArray<CharElement> chars, 
            NativeArray<GlyphElement> glyphs,
            NativeList<TextUtil.LineInfo> lines,
            NativeList<Vertex> vertices,
            NativeList<Index> indices,
            FontFaceInfo fontFace,
            Dimension dimension,
            ScreenSpace screenSpace,
            float2 rootScale,
            TextOptions options,
            float4 color,
            int submeshIndex,
            EntityCommandBuffer commandBuffer,
            NativeHashMap<int, Slice> submeshSliceMap,
            ref int2 spans) {

            var bl = (ushort)spans.x;

            var isBold          = options.Style == FontStyles.Bold;
            var fontScale       = math.select(1f, options.Size / fontFace.PointSize, options.Size > 0);
            var spaceMultiplier = 1f + math.select(fontFace.NormalStyle.y, fontFace.BoldStyle.y, isBold) * 0.01f;
            var padding         = fontScale * spaceMultiplier;

            TextUtil.CountLines(chars, glyphs, dimension, padding, ref lines);

            var totalLineHeight = lines.Length * fontFace.LineHeight * fontScale * screenSpace.Scale.y;
            var stylePadding = TextUtil.SelectStylePadding(options, fontFace);

            var extents = dimension.Extents() * screenSpace.Scale * rootScale;
            var heights = new float3(fontFace.LineHeight, fontFace.AscentLine, fontFace.DescentLine) * 
                screenSpace.Scale.y * rootScale.y;

            var start = new float2(
                TextUtil.GetHorizontalAlignment(options.Alignment, extents, lines[0].LineWidth * 
                    screenSpace.Scale.x * rootScale.x),
                TextUtil.GetVerticalAlignment(
                    heights, fontScale, options.Alignment, extents, totalLineHeight, lines.Length)) + 
                    screenSpace.Translation;

            for (int i = 0, row = 0; i < chars.Length; i++) {
                var c = chars[i];

                if (!FindGlyphWithChar(glyphs, c, out GlyphElement glyph)) {
                    continue;
                }

                if (row < lines.Length && i == lines[row].StartIndex) {
                    var height = fontFace.LineHeight * fontScale * screenSpace.Scale.y * rootScale.y * 
                        math.select(0, 1f, row > 0);
                    start.y -= height;
                    start.x = TextUtil.GetHorizontalAlignment(
                        options.Alignment, 
                        extents, 
                        lines[row].LineWidth * screenSpace.Scale.x * rootScale.x) + screenSpace.Translation.x;
                    row++;
                }

                var xPos = start.x + (glyph.Bearings.x - stylePadding) * fontScale * 
                    screenSpace.Scale.x * rootScale.x;
                var yPos = start.y - (glyph.Size.y - glyph.Bearings.y - stylePadding) * fontScale * 
                    screenSpace.Scale.y * rootScale.y;
                var size = (glyph.Size + new float2(stylePadding * 2)) * fontScale * screenSpace.Scale * rootScale.y;
                var uv1  = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);

                var canvasScale = rootScale.x * screenSpace.Scale.x / 4 * 3;
                var uv2         = new float2(glyph.Scale) * math.select(canvasScale, -canvasScale, isBold);
                var normal      = new float3(1, 0, 0);

                vertices.Add(new Vertex {
                    Position = new float3(xPos, yPos, 0),
                    Normal   = normal,
                    Color    = color,
                    UV1      = uv1.c0,
                    UV2      = uv2
                });
                vertices.Add(new Vertex {
                    Position = new float3(xPos, yPos + size.y, 0),
                    Normal   = normal,
                    Color    = color,
                    UV1      = uv1.c1,
                    UV2      = uv2
                });
                vertices.Add(new Vertex {
                    Position = new float3(xPos + size.x, yPos + size.y, 0),
                    Normal   = normal,
                    Color    = color,
                    UV1      = uv1.c2,
                    UV2      = uv2
                });
                vertices.Add(new Vertex {
                    Position = new float3(xPos + size.x, yPos, 0),
                    Normal   = normal,
                    Color    = color,
                    UV1      = uv1.c3,
                    UV2      = uv2
                });

                var tl = (ushort)(bl + 1);
                var tr = (ushort)(bl + 2);
                var br = (ushort)(bl + 3);

                indices.Add(new Index { Value = bl }); // 0
                indices.Add(new Index { Value = tl }); // 1
                indices.Add(new Index { Value = tr }); // 2

                indices.Add(new Index { Value = bl }); // 0
                indices.Add(new Index { Value = tr }); // 2
                indices.Add(new Index { Value = br }); // 3

                start.x += glyph.Advance * padding * screenSpace.Scale.x * rootScale.x;

                // Update the bottom left index, because we add 4 new vertices.
                bl += 4;
            }

            // Update the spans of the indices and vertices
            commandBuffer.SetComponent(textEntity, new MeshDataSpan {
                IndexSpan = new int2(spans.y, indices.Length),
                VertexSpan = new int2(spans.x, vertices.Length)
            });

            var key = submeshIndex.GetHashCode() ^ canvasEntity.GetHashCode();
            var currentSpan = new int2(vertices.Length, indices.Length);

            if (submeshSliceMap.TryGetValue(key, out Slice slice)) {
                slice.VertexSpan     = new int2(slice.VertexSpan.x, slice.VertexSpan.y + currentSpan.x);
                slice.IndexSpan      = new int2(slice.IndexSpan.x, slice.IndexSpan.y + currentSpan.y);
                submeshSliceMap[key] = slice;
            } else {
                submeshSliceMap.Add(key, new Slice {
                    IndexSpan    = new int2(spans.y, currentSpan.y),
                    VertexSpan   = new int2(spans.x, currentSpan.x),
                    Canvas       = canvasEntity,
                    SubmeshIndex = submeshIndex
                });
            }

            // Update the new offset.
            spans += currentSpan;
        }

    }

    internal struct Slice {
        internal int2 VertexSpan;
        internal int2 IndexSpan;
        internal Entity Canvas;
        internal int SubmeshIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubmeshSliceElement ToSubmeshSlice() {
            return new SubmeshSliceElement {
                IndexSpan = IndexSpan,
                VertexSpan = VertexSpan
            };
        }
    }

    [BurstCompile]
    internal unsafe struct ConsolidateAndBuildDynamicTextJob : IJob {

        public EntityCommandBuffer CommandBuffer;

        public NativeHashMap<Entity, int2> StaticSpans;

        public NativeHashMap<int, Slice> SubmeshSliceMap;

        // ReadOnly Containers 
        // --------------------------------------------------------------
        [ReadOnly]
        public PerThreadContainer<EntityContainer> DynamicText;

        // Canvas Data
        // --------------------------------------------------------------
        [ReadOnly]
        public ComponentDataFromEntity<ScreenSpace> ScreenSpaces;

        // Font Data
        // --------------------------------------------------------------
        [ReadOnly]
        public BufferFromEntity<GlyphElement> GlyphBuffers;

        [ReadOnly]
        public ComponentDataFromEntity<LinkedTextFontEntity> LinkedTextFont;

        [ReadOnly]
        public ComponentDataFromEntity<FontFaceInfo> FontFaces;

        // Text Data
        // --------------------------------------------------------------
        [ReadOnly]
        public BufferFromEntity<CharElement> CharBuffers;

        [ReadOnly]
        public ComponentDataFromEntity<RootCanvasReference> Roots;

        [ReadOnly]
        public ComponentDataFromEntity<SubmeshIndex> SubmeshIndices;

        [ReadOnly]
        public ComponentDataFromEntity<AppliedColor> AppliedColors;

        [ReadOnly]
        public ComponentDataFromEntity<TextOptions> TextOptions;

        [ReadOnly]
        public ComponentDataFromEntity<Dimension> Dimensions;

        // Mesh Data
        // --------------------------------------------------------------
        public BufferFromEntity<Vertex> Vertices;

        public BufferFromEntity<Index> Indices;

        public void Execute() {
            var minPriorityQueue = new UnsafeMinPriorityQueue<EntityPriority>(Allocator.Temp, 100);

            for (int i = 0; i < DynamicText.Length; i++) {
                UnsafeList<EntityContainer>* texts = DynamicText.Ptr + i;

                for (int j = 0; j < texts->Length; j++) {
                    var entity = texts->ElementAt(j);
                    var submeshIndex = SubmeshIndices[entity].Value;

                    minPriorityQueue.Add(new EntityPriority {
                        Entity       = entity,
                        SubmeshIndex = submeshIndex
                    });
                }
            }

            var tempVertices = new NativeList<Vertex>(500, Allocator.Temp);
            var tempIndices  = new NativeList<Index>(1500, Allocator.Temp);
            var lines        = new NativeList<TextUtil.LineInfo>(10, Allocator.Temp);

            while (minPriorityQueue.Length > 0) {

            // for (int i = 0; i < minPriorityQueue.Length; i++) {
                var entityPriority = minPriorityQueue.Pull();
                var textEntity     = entityPriority.Entity;

                // Get the canvas data
                var rootEntity = Roots[textEntity].Value;
                var staticSpan = StaticSpans[rootEntity];
                var vertices   = Vertices[rootEntity];
                var indices    = Indices[rootEntity];
                var rootSpace  = ScreenSpaces[rootEntity];

                // Get all of the text data we need
                var chars       = CharBuffers[textEntity].AsNativeArray();
                var dimension   = Dimensions[textEntity];
                var screenSpace = ScreenSpaces[textEntity];
                var textOptions = TextOptions[textEntity];
                var color       = AppliedColors[textEntity].Value.ToNormalizedFloat4();

                var linked   = LinkedTextFont[textEntity].Value;
                var fontFace = FontFaces[linked];
                var glyphs   = GlyphBuffers[linked].AsNativeArray();

                var submeshIndex = entityPriority.SubmeshIndex;
                var originalSpan = staticSpan;

                TextBuildUtility.CreateVertexForChars(
                    rootEntity,
                    textEntity,
                    chars, 
                    glyphs, 
                    lines, 
                    tempVertices, 
                    tempIndices,
                    fontFace, 
                    dimension, 
                    screenSpace, 
                    rootSpace.Scale, 
                    textOptions, 
                    color,
                    submeshIndex,
                    CommandBuffer,
                    SubmeshSliceMap,
                    ref staticSpan);

                // Update the hashmap with the new spans so the next entities can batch.
                StaticSpans[rootEntity] = staticSpan;

                TextBuildUtility.CopyToBuffer(vertices, indices, tempVertices, tempIndices, originalSpan);

                tempVertices.Clear();
                tempIndices.Clear();
                lines.Clear();
                // TODO: Canvas needs to rebuild itself because of the dynamic elements.
            }

            minPriorityQueue.Dispose();
        }
    }

    [BurstCompile]
    internal unsafe struct BuildDynamicTextJob : IJob {

        public EntityCommandBuffer CommandBuffer;

        public NativeHashMap<Entity, int2> StaticSpans;

        public NativeHashMap<int, Slice> SubmeshSliceMap;

        // ReadOnly Containers 
        // --------------------------------------------------------------
        [ReadOnly]
        public UnsafeMinPriorityQueue<EntityPriority> PriorityQueue;

        // Canvas Data
        // --------------------------------------------------------------
        [ReadOnly]
        public ComponentDataFromEntity<ScreenSpace> ScreenSpaces;

        // Font Data
        // --------------------------------------------------------------
        [ReadOnly]
        public BufferFromEntity<GlyphElement> GlyphBuffers;

        [ReadOnly]
        public ComponentDataFromEntity<LinkedTextFontEntity> LinkedTextFont;

        [ReadOnly]
        public ComponentDataFromEntity<FontFaceInfo> FontFaces;

        // Text Data
        // --------------------------------------------------------------
        [ReadOnly]
        public BufferFromEntity<CharElement> CharBuffers;

        [ReadOnly]
        public ComponentDataFromEntity<RootCanvasReference> Roots;

        [ReadOnly]
        public ComponentDataFromEntity<SubmeshIndex> SubmeshIndices;

        [ReadOnly]
        public ComponentDataFromEntity<AppliedColor> AppliedColors;

        [ReadOnly]
        public ComponentDataFromEntity<TextOptions> TextOptions;

        [ReadOnly]
        public ComponentDataFromEntity<Dimension> Dimensions;

        // Mesh Data
        // --------------------------------------------------------------
        public BufferFromEntity<Vertex> Vertices;

        public BufferFromEntity<Index> Indices;

        public void Execute() {
            var tempVertices = new NativeList<Vertex>(500, Allocator.Temp);
            var tempIndices  = new NativeList<Index>(1500, Allocator.Temp);
            var lines        = new NativeList<TextUtil.LineInfo>(10, Allocator.Temp);

            UnityEngine.Debug.Log($"Build Text Priority Queue Length: {PriorityQueue.Length}");

            while (PriorityQueue.Length > 0) {

            // for (int i = 0; i < minPriorityQueue.Length; i++) {
                var entityPriority = PriorityQueue.Pull();
                var textEntity     = entityPriority.Entity;

                // Get the canvas data
                var rootEntity = Roots[textEntity].Value;
                var staticSpan = StaticSpans[rootEntity];
                var vertices   = Vertices[rootEntity];
                var indices    = Indices[rootEntity];
                var rootSpace  = ScreenSpaces[rootEntity];

                // Get all of the text data we need
                var chars       = CharBuffers[textEntity].AsNativeArray();
                var dimension   = Dimensions[textEntity];
                var screenSpace = ScreenSpaces[textEntity];
                var textOptions = TextOptions[textEntity];
                var color       = AppliedColors[textEntity].Value.ToNormalizedFloat4();

                var linked   = LinkedTextFont[textEntity].Value;
                var fontFace = FontFaces[linked];
                var glyphs   = GlyphBuffers[linked].AsNativeArray();

                var submeshIndex = entityPriority.SubmeshIndex;
                var originalSpan = staticSpan;

                TextBuildUtility.CreateVertexForChars(
                    rootEntity,
                    textEntity,
                    chars, 
                    glyphs, 
                    lines, 
                    tempVertices, 
                    tempIndices,
                    fontFace, 
                    dimension, 
                    screenSpace, 
                    rootSpace.Scale, 
                    textOptions, 
                    color,
                    submeshIndex,
                    CommandBuffer,
                    SubmeshSliceMap,
                    ref staticSpan);

                // Update the hashmap with the new spans so the next entities can batch.
                StaticSpans[rootEntity] = staticSpan;

                TextBuildUtility.CopyToBuffer(vertices, indices, tempVertices, tempIndices, originalSpan);

                tempVertices.Clear();
                tempIndices.Clear();
                lines.Clear();
                // TODO: Canvas needs to rebuild itself because of the dynamic elements.
            }
        }
    }
}
