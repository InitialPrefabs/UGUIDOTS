using System;
using System.Collections.Generic;
using TMPro;
using UGUIDOTS.Collections;
using UGUIDOTS.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public unsafe class BuildRenderHierarchySystem : SystemBase {


        private struct EntityContainer : IStruct<EntityContainer> {
            public Entity Value;

            public static implicit operator EntityContainer(Entity value) => new EntityContainer { Value = value };
            public static implicit operator Entity(EntityContainer value) => value.Value;
        }

        [BurstCompile]
        private unsafe struct CollectEntitiesJob : IJobChunk {

#pragma warning disable CS0649
            [NativeSetThreadIndex]
            public int NativeThreadIndex;
#pragma warning restore CS0649

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public EntityTypeHandle EntityType;

            // Canvases
            // -----------------------------------------
            public BufferTypeHandle<Vertex> VertexBufferType;

            // Universal
            // -----------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<MeshDataSpan> MeshDataSpans;

            // Images
            // -----------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<SpriteData> SpriteData;

            [ReadOnly]
            public ComponentDataFromEntity<Stretch> Stretched;

            // Text
            // -----------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<DynamicTextTag> DynamicTexts;

            [ReadOnly]
            public BufferFromEntity<CharElement> CharBuffers;

            // Parallel Containers
            // -----------------------------------------
            [NativeDisableUnsafePtrRestriction]
            public UnsafeList<EntityContainer>* ImageContainer;

            [NativeDisableUnsafePtrRestriction]
            public UnsafeList<EntityContainer>* StaticTextContainer;

            [NativeDisableUnsafePtrRestriction]
            public UnsafeList<EntityContainer>* DynamicTextContainer;

            [WriteOnly]
            public NativeHashMap<Entity, IntPtr>.ParallelWriter CanvasVertexMap;
            
            [WriteOnly]
            public NativeHashMap<Entity, int2>.ParallelWriter StaticElementCount;

            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);
                var vertexBuffers = chunk.GetBufferAccessor(VertexBufferType);

                var index = NativeThreadIndex - 1;

                for (int i = 0; i < chunk.Count; i++) {
                    var entity   = entities[i];
                    var children = Children[entity].AsNativeArray().AsReadOnly();

                    UnsafeList<EntityContainer>* img        = ImageContainer + index;
                    UnsafeList<EntityContainer>* staticTxt  = StaticTextContainer + index;
                    UnsafeList<EntityContainer>* dynamicTxt = DynamicTextContainer + index;

                    var staticDataSpan = new int2();

                    RecurseChildrenDetermineType(children, entity, img, staticTxt, dynamicTxt, ref staticDataSpan);

                    var vertices = vertexBuffers[i];

                    // Add the vertex ptr so we can write to it in a later job.
                    void* vertexPtr = vertices.GetUnsafePtr();
                    CanvasVertexMap.TryAdd(entity, new IntPtr(vertexPtr));

                    // Add the static data span, so when we build dynamic elements, we can batch it together.
                    StaticElementCount.TryAdd(entity, staticDataSpan);

                    CommandBuffer.AddComponent<RebuildMeshTag>(firstEntityIndex + i, entity);
                }
            }

            void RecurseChildrenDetermineType(
                NativeArray<Child>.ReadOnly children, 
                Entity root, 
                UnsafeList<EntityContainer>* imgContainer, 
                UnsafeList<EntityContainer>* staticTxtContainer,
                UnsafeList<EntityContainer>* dynamicTxtContainer,
                ref int2 count) {

                for (int i = 0; i < children.Length; i++) {
                    var entity = children[i].Value;
                    
                    var meshSpan = MeshDataSpans[entity];

                    // Add the spans of all static images
                    if (SpriteData.HasComponent(entity)) {
                        imgContainer->Add(entity);
                        count += new int2(meshSpan.VertexSpan.y, meshSpan.IndexSpan.y);
                    }

                    if (CharBuffers.HasComponent(entity)) {
                        if (DynamicTexts.HasComponent(entity)) {
                            // Skip the dynamic text because the # of vertices/indices will change
                            dynamicTxtContainer->Add(entity);
                        } else {
                            // Add the spans of all static text
                            staticTxtContainer->Add(entity);
                            count += new int2(meshSpan.VertexSpan.y, meshSpan.IndexSpan.y);
                        }
                    }

                    if (Children.HasComponent(entity)) {
                        var grandChildren = Children[entity].AsNativeArray().AsReadOnly();
                        RecurseChildrenDetermineType(
                            grandChildren, 
                            root, 
                            imgContainer, 
                            staticTxtContainer, 
                            dynamicTxtContainer,
                            ref count);
                    }
                }
            }
        }

        // NOTE: Assume all static
        [BurstCompile]
        private struct BuildImageJob : IJob {

            public PerThreadContainer<EntityContainer> ThreadContainer;

            [ReadOnly]
            public NativeHashMap<Entity, IntPtr> CanvasMap;

            [ReadOnly]
            public ComponentDataFromEntity<SpriteData> SpriteData;

            [ReadOnly]
            public ComponentDataFromEntity<DefaultSpriteResolution> SpriteResolutions;

            [ReadOnly]
            public ComponentDataFromEntity<Dimension> Dimensions;

            [ReadOnly]
            public ComponentDataFromEntity<AppliedColor> Colors;

            [ReadOnly]
            public ComponentDataFromEntity<ScreenSpace> ScreenSpaces;

            [ReadOnly]
            public ComponentDataFromEntity<MeshDataSpan> MeshDataSpans;

            [ReadOnly]
            public ComponentDataFromEntity<RootCanvasReference> Root;

            [ReadOnly]
            public ComponentDataFromEntity<Stretch> Stretched;

            void UpdateImageVertices(Entity entity, NativeArray<Vertex> tempImageData, bool useRootScale) {
                var root = Root[entity].Value;

                // Get the root data
                var vertexPtr     = (Vertex*)CanvasMap[root].ToPointer();
                var rootTransform = ScreenSpaces[root];

                // Build the image data
                var spriteData  = SpriteData[entity];
                var resolution  = SpriteResolutions[entity];
                var dimension   = Dimensions[entity];
                var screenSpace = ScreenSpaces[entity];
                var color       = Colors[entity].Value.ToNormalizedFloat4();

                var scale  = math.select(1, rootTransform.Scale.x, useRootScale);
                var minMax = ImageUtils.CreateImagePositionData(resolution, spriteData, dimension, screenSpace, scale);
                var span   = MeshDataSpans[entity];

                ImageUtils.FillVertexSpan(tempImageData, minMax, spriteData, color);

                var dst  = (Vertex*)vertexPtr + span.VertexSpan.x;
                var size = UnsafeUtility.SizeOf<Vertex>() * span.VertexSpan.y;
                UnsafeUtility.MemCpy(dst, tempImageData.GetUnsafePtr(), size);
            }

            public void Execute() {
                var tempImageData = new NativeArray<Vertex>(4, Allocator.Temp);

                for (int i = 0; i < ThreadContainer.Length; i++) {
                    var list = ThreadContainer.Ptr[i];

                    for (int k = 0; k < list.Length; k++) {
                        var useRoot = Stretched.HasComponent(list[k].Value);
                        UpdateImageVertices(list[k], tempImageData, !useRoot);
                    }
                }
            }
        }

        [BurstCompile]
        private struct BuildTextJob : IJobParallelFor {

            public PerThreadContainer<EntityContainer> TextEntities;

            [ReadOnly]
            public NativeHashMap<Entity, IntPtr> CanvasMap;

            // Font info
            // ------------------------------------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<LinkedTextFontEntity> LinkedTextFonts;
            
            [ReadOnly]
            public BufferFromEntity<GlyphElement> Glyphs;

            [ReadOnly]
            public ComponentDataFromEntity<FontFaceInfo> FontFace;

            // Text info
            // ------------------------------------------------------------------
            [ReadOnly]
            public BufferFromEntity<CharElement> CharBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<Dimension> Dimensions;

            [ReadOnly]
            public ComponentDataFromEntity<ScreenSpace> ScreenSpace;
            
            [ReadOnly]
            public ComponentDataFromEntity<TextOptions> TextOptions;

            [ReadOnly]
            public ComponentDataFromEntity<AppliedColor> AppliedColors;

            [ReadOnly]
            public ComponentDataFromEntity<MeshDataSpan> Spans;

            [ReadOnly]
            public ComponentDataFromEntity<RootCanvasReference> Roots;

            public void Execute(int index) {
                var lines = new NativeList<TextUtil.LineInfo>(10, Allocator.Temp);
                var tempVertexData = new NativeList<Vertex>(Allocator.Temp);
                
                var list = TextEntities.Ptr[index];
                for (int i = 0; i < list.Length; i++) {
                    var entity = list[i];
                    var linked = LinkedTextFonts[entity].Value;

                    var glyphs   = Glyphs[linked].AsNativeArray();
                    var fontFace = FontFace[linked];

                    var root = Roots[entity].Value;

                    var chars       = CharBuffers[entity].AsNativeArray();
                    var dimension   = Dimensions[entity];
                    var screenSpace = ScreenSpace[entity];
                    var textOptions = TextOptions[entity];
                    var color       = AppliedColors[entity].Value.ToNormalizedFloat4();
                    var rootScreen  = ScreenSpace[root];

                    CreateVertexForChars(
                        chars, 
                        glyphs, 
                        lines, 
                        tempVertexData, 
                        fontFace, 
                        dimension, 
                        screenSpace, 
                        rootScreen.Scale,
                        textOptions, 
                        color);

                    var span      = Spans[entity];
                    var vertexPtr = (Vertex*)CanvasMap[root].ToPointer();

                    var dst = (Vertex*)vertexPtr + span.VertexSpan.x;
                    var size = span.VertexSpan.y * UnsafeUtility.SizeOf<Vertex>();
                    UnsafeUtility.MemCpy(dst, tempVertexData.GetUnsafePtr(), size);

                    tempVertexData.Clear();
                    lines.Clear();
                }
            }

            void CreateVertexForChars(
                NativeArray<CharElement> chars,
                NativeArray<GlyphElement> glyphs,
                NativeList<TextUtil.LineInfo> lines,
                NativeList<Vertex> vertices,
                FontFaceInfo fontFace,
                Dimension dimension,
                ScreenSpace screenSpace,
                float2 rootScale,
                TextOptions options,
                float4 color) {
    
                var isBold = options.Style == FontStyles.Bold;
                var fontScale = math.select(1f, options.Size / fontFace.PointSize, options.Size > 0);
                var spaceMultiplier = 1f + math.select(fontFace.NormalStyle.y, fontFace.BoldStyle.y, isBold) * 0.01f;
                var padding = fontScale * spaceMultiplier;
    
                TextUtil.CountLines(chars, glyphs, dimension, padding, ref lines);
    
                var totalLineHeight = lines.Length * fontFace.LineHeight * fontScale * screenSpace.Scale.y;
                var stylePadding = TextUtil.SelectStylePadding(options, fontFace);
    
                var extents = dimension.Extents() * screenSpace.Scale * rootScale;
                var heights = new float3(fontFace.LineHeight, fontFace.AscentLine, fontFace.DescentLine) *
                    screenSpace.Scale.y * rootScale.y;
    
                var start = new float2(
                    TextUtil.GetHorizontalAlignment(options.Alignment, extents, lines[0].LineWidth *
                        screenSpace.Scale.x * rootScale.x),
                    TextUtil.GetVerticalAlignment(heights, fontScale, options.Alignment, extents, totalLineHeight, lines.Length)
                ) + screenSpace.Translation;
    
                var normal = new float3(1, 0, 0);
    
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
                    var uv1 = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);
    
                    var canvasScale = rootScale.x * screenSpace.Scale.x / 4 * 3;
                    var uv2 = new float2(glyph.Scale) * math.select(canvasScale, -canvasScale, isBold);
    
                    vertices.Add(new Vertex {
                        Position = new float3(xPos, yPos, 0),
                        Normal = normal,
                        Color = color,
                        UV1 = uv1.c0,
                        UV2 = uv2
                    });
                    vertices.Add(new Vertex {
                        Position = new float3(xPos, yPos + size.y, 0),
                        Normal = normal,
                        Color = color,
                        UV1 = uv1.c1,
                        UV2 = uv2
                    });
                    vertices.Add(new Vertex {
                        Position = new float3(xPos + size.x, yPos + size.y, 0),
                        Normal = normal,
                        Color = color,
                        UV1 = uv1.c2,
                        UV2 = uv2
                    });
                    vertices.Add(new Vertex {
                        Position = new float3(xPos + size.x, yPos, 0),
                        Normal = normal,
                        Color = color,
                        UV1 = uv1.c3,
                        UV2 = uv2
                    });
    
                    start.x += glyph.Advance * padding * screenSpace.Scale.x * rootScale.x;
                }
            }
        
            bool FindGlyphWithChar(NativeArray<GlyphElement> glyphs, char c, out GlyphElement glyph) {
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
        }
        
        [BurstCompile]
        private struct CollectAndSeparateEntityJob : IJob {

            [ReadOnly]
            public ComponentDataFromEntity<RootCanvasReference> RootCanvasReferences;
            
            public PerThreadContainer<EntityContainer> TextEntities;

            public NativeMultiHashMap<Entity, Entity> SortedCanvasEntities;

            public void Execute() {
                for (int i = 0; i < TextEntities.Length; i++) {
                    UnsafeList<EntityContainer>* text = TextEntities.Ptr + i;
                    for (int j = 0; j < text->Length; j++) {
                        var currentTextEntity = text->Ptr[j].Value;

                        if (RootCanvasReferences.HasComponent(currentTextEntity)) {
                            var rootCanvas = RootCanvasReferences[currentTextEntity].Value;
                            SortedCanvasEntities.Add(rootCanvas, currentTextEntity);
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private struct BuildDynamicTextJob : IJob {

            private struct Pair {
                public Entity Entity;
                public int Index;
            }

            private struct CompareSubmeshIndex : IComparer<Pair> {
                public int Compare(Pair x, Pair y) {
                    if (x.Index > y.Index) {
                        return 1;
                    }
    
                    if (x.Index < y.Index) {
                        return -1;
                    }
    
                    return 0;
                }
            }

            public PerThreadContainer<EntityContainer> TextEntities;

            public NativeHashMap<Entity, int2> AccumulatedStaticSpans;

            public BufferFromEntity<Vertex> VertexBuffers;

            public BufferFromEntity<Index> IndexBuffers;

            [ReadOnly]
            public NativeMultiHashMap<Entity, Entity> CollectedEntities;

            // [ReadOnly]
            // public NativeHashMap<Entity, IntPtr> CanvasMap;

            // Font info
            // ------------------------------------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<LinkedTextFontEntity> LinkedTextFonts;
            
            [ReadOnly]
            public BufferFromEntity<GlyphElement> Glyphs;

            [ReadOnly]
            public ComponentDataFromEntity<FontFaceInfo> FontFace;

            // Text info
            // ------------------------------------------------------------------
            public ComponentDataFromEntity<MeshDataSpan> Spans;

            [ReadOnly]
            public ComponentDataFromEntity<SubmeshIndex> SubmeshIndices;

            [ReadOnly]
            public BufferFromEntity<CharElement> CharBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<Dimension> Dimensions;

            [ReadOnly]
            public ComponentDataFromEntity<ScreenSpace> ScreenSpace;
            
            [ReadOnly]
            public ComponentDataFromEntity<TextOptions> TextOptions;

            [ReadOnly]
            public ComponentDataFromEntity<AppliedColor> AppliedColors;

            [ReadOnly]
            public ComponentDataFromEntity<RootCanvasReference> Roots;

            public void Execute() {
                var keys           = CollectedEntities.GetKeyArray(Allocator.Temp);
                var toSort         = new NativeList<Pair>(10, Allocator.Temp);
                var tempVertexData = new NativeList<Vertex>(10, Allocator.Temp);
                var lines          = new NativeList<TextUtil.LineInfo>(10, Allocator.Temp);
                var comparer       = new CompareSubmeshIndex();

                for (int i = 0; i < keys.Length; i++) {
                    toSort.Clear();
                    var canvasEntity = keys[i];

                    CollectedEntities.TryGetFirstValue(canvasEntity, 
                        out Entity dynTextEntity, 
                        out NativeMultiHashMapIterator<Entity> it);

                    var submesh = SubmeshIndices[dynTextEntity].Value;
                    toSort.Add(new Pair { 
                        Entity = dynTextEntity,
                        Index  = submesh
                    });

                    while (CollectedEntities.TryGetNextValue(out dynTextEntity, ref it)) {
                        submesh = SubmeshIndices[dynTextEntity].Value;
                        toSort.Add(new Pair { 
                            Entity = dynTextEntity,
                            Index  = submesh
                        });
                    }

                    toSort.Sort(comparer);

                    // Do the actual text building
                    for (int j = 0; j < toSort.Length; j++) {
                        var dynamicTextEntity = toSort[j].Entity;
                        var linked = LinkedTextFonts[dynamicTextEntity].Value;

                        var glyphs   = Glyphs[linked].AsNativeArray();
                        var fontFace = FontFace[linked];

                        var chars       = CharBuffers[dynamicTextEntity].AsNativeArray();
                        var dimension   = Dimensions[dynamicTextEntity];
                        var screenSpace = ScreenSpace[dynamicTextEntity];
                        var textOptions = TextOptions[dynamicTextEntity];
                        var color       = AppliedColors[dynamicTextEntity].Value.ToNormalizedFloat4();
                        var rootScreen  = ScreenSpace[canvasEntity];

                        // TODO: Rebuild the vertices and rebuild the indices
                    }

                    // TODO: After finishing the canvas, copy the slices that intersect and add the remaining

                    // Clear the sorted entities so we can reuse it for the next iteration
                    toSort.Clear();
                    tempVertexData.Clear();
                }
            }

            void CreateVertexForChars(
                NativeArray<CharElement> chars,
                NativeArray<GlyphElement> glyphs,
                NativeList<TextUtil.LineInfo> lines,
                ref NativeList<Vertex> vertices,
                ref NativeList<Index> indices,
                FontFaceInfo fontFace,
                Dimension dimension,
                ScreenSpace screenSpace,
                float2 rootScale,
                TextOptions options,
                float4 color,
                ref int2 staticSpan) {

                var bl = (ushort)staticSpan.x;

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
                    TextUtil.GetVerticalAlignment(heights, fontScale, options.Alignment, extents, totalLineHeight, lines.Length)
                ) + screenSpace.Translation;
    
                var normal = new float3(1, 0, 0);
    
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
                    var uv1 = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);
    
                    var canvasScale = rootScale.x * screenSpace.Scale.x / 4 * 3;
                    var uv2 = new float2(glyph.Scale) * math.select(canvasScale, -canvasScale, isBold);
    
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

                    // TODO: Resize the indices to the static elements.
    
                    indices.Add(new Index { Value = bl }); // 0
                    indices.Add(new Index { Value = tl }); // 1
                    indices.Add(new Index { Value = tr }); // 2
    
                    indices.Add(new Index { Value = bl }); // 0
                    indices.Add(new Index { Value = tr }); // 2
                    indices.Add(new Index { Value = br }); // 3
    
                    start.x += glyph.Advance * padding * screenSpace.Scale.x * rootScale.x;
                }
                
                // Update the static span because we want to ensure that the next set of
                // text will be at an offset of the previous.
                staticSpan = new int2(vertices.Length, staticSpan.y);
            }

            bool FindGlyphWithChar(NativeArray<GlyphElement> glyphs, char c, out GlyphElement glyph) {
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
        }

        private EntityQuery canvasQuery;
        private EntityQuery imageQuery;
        private EntityQuery textQuery;
        private EntityCommandBufferSystem commandBufferSystem;

        private PerThreadContainer<EntityContainer> perThreadImageContainer;
        private PerThreadContainer<EntityContainer> perThreadStaticTextContainer;
        private PerThreadContainer<EntityContainer> perThreadDynamicTextContainer;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<ReferenceResolution>(), ComponentType.ReadOnly<Child>(),
                    ComponentType.ReadOnly<OnResolutionChangeTag>()
                }
            });

            imageQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<SpriteData>() }
            });

            textQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<CharElement>() }
            });

            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireForUpdate(canvasQuery);

            perThreadImageContainer = new PerThreadContainer<EntityContainer>(
                JobsUtility.JobWorkerCount + 1, 
                20, 
                Allocator.Persistent);

            perThreadStaticTextContainer = new PerThreadContainer<EntityContainer>(
                JobsUtility.JobWorkerCount + 1, 
                20, 
                Allocator.Persistent);
        }

        protected override void OnDestroy() {
            perThreadImageContainer.Dispose();
            perThreadStaticTextContainer.Dispose();
            perThreadDynamicTextContainer.Dispose();
        }

        protected override void OnUpdate() {
            perThreadImageContainer.Reset();
            perThreadStaticTextContainer.Reset();
            perThreadDynamicTextContainer.Reset();

            var charBuffers = GetBufferFromEntity<CharElement>();
            var spriteData  = GetComponentDataFromEntity<SpriteData>(true);
            var children    = GetBufferFromEntity<Child>(true);

            var dimensions    = GetComponentDataFromEntity<Dimension>(true);
            var colors        = GetComponentDataFromEntity<AppliedColor>(true);
            var spans         = GetComponentDataFromEntity<MeshDataSpan>(true);
            var screenSpaces  = GetComponentDataFromEntity<ScreenSpace>(true);
            var resolutions   = GetComponentDataFromEntity<DefaultSpriteResolution>(true);
            var stretch       = GetComponentDataFromEntity<Stretch>(true);
            var root          = GetComponentDataFromEntity<RootCanvasReference>(true);

            var canvasCount = canvasQuery.CalculateEntityCount();

            var canvasMap      = new NativeHashMap<Entity, IntPtr>(canvasCount, Allocator.TempJob);
            var staticCountMap = new NativeHashMap<Entity, int2>(canvasCount, Allocator.TempJob);
            var commandBuffer  = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            // TODO: Collect -> Build Image + Build Text can work here
            Dependency               = new CollectEntitiesJob {
                CommandBuffer        = commandBuffer,
                CharBuffers          = charBuffers,
                Children             = children,
                Stretched            = stretch,
                EntityType           = GetEntityTypeHandle(),
                SpriteData           = spriteData,
                VertexBufferType     = GetBufferTypeHandle<Vertex>(false),
                DynamicTexts         = GetComponentDataFromEntity<DynamicTextTag>(true),
                MeshDataSpans        = GetComponentDataFromEntity<MeshDataSpan>(true),
                ImageContainer       = perThreadImageContainer.Ptr,
                StaticTextContainer  = perThreadStaticTextContainer.Ptr,
                DynamicTextContainer = perThreadDynamicTextContainer.Ptr,
                CanvasVertexMap      = canvasMap.AsParallelWriter(),
                StaticElementCount   = staticCountMap.AsParallelWriter()
            }.ScheduleParallel(canvasQuery, Dependency);

            var imgDeps           = new BuildImageJob {
                Root              = root,
                Colors            = colors,
                Dimensions        = dimensions,
                MeshDataSpans     = spans,
                ScreenSpaces      = screenSpaces,
                SpriteData        = spriteData,
                SpriteResolutions = resolutions,
                Stretched         = stretch,
                ThreadContainer   = perThreadImageContainer,
                CanvasMap         = canvasMap
            }.Schedule(Dependency);

            var fontFaces   = GetComponentDataFromEntity<FontFaceInfo>(true);
            var glyphs      = GetBufferFromEntity<GlyphElement>(true);
            var linkedFonts = GetComponentDataFromEntity<LinkedTextFontEntity>(true);
            var textOptions = GetComponentDataFromEntity<TextOptions>(true);

            var staticTextDeps  = new BuildTextJob {
                AppliedColors   = colors,
                CharBuffers     = charBuffers,
                Dimensions      = dimensions,
                FontFace        = fontFaces,
                Glyphs          = glyphs,
                LinkedTextFonts = linkedFonts,
                ScreenSpace     = screenSpaces,
                Spans           = spans,
                TextOptions     = textOptions,
                Roots           = root,
                TextEntities    = perThreadStaticTextContainer,
                CanvasMap       = canvasMap
            }.Schedule(perThreadImageContainer.Length, 1, Dependency);

            // TODO: Build the dynamic text.
            // TODO: Ideally only dynamic text needs to be rebuilt if nothing else changes.
            var dynamicTextDeps = new BuildDynamicTextJob {
                
            }.Schedule(Dependency);

            var combinedDeps       = JobHandle.CombineDependencies(imgDeps, staticTextDeps, dynamicTextDeps);
            var canvasDisposalDeps = canvasMap.Dispose(combinedDeps);
            var staticCountMapDeps = staticCountMap.Dispose(combinedDeps);

            Dependency = JobHandle.CombineDependencies(canvasDisposalDeps, staticCountMapDeps);
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
