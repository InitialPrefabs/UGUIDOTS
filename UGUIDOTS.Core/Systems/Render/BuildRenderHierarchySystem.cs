using System;
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

        struct EntityContainer : IStruct<EntityContainer> {
            public Entity Value;

            public static implicit operator EntityContainer(Entity value) => new EntityContainer { Value = value };
            public static implicit operator Entity(EntityContainer value) => value.Value;
        }

        struct EntityPriority : IStruct<EntityPriority>, IPrioritize<EntityPriority> {

            public Entity Entity;
            public int SubmeshIndex;

            public int Priority() {
                return SubmeshIndex;
            }
        }

        [BurstCompile]
        unsafe struct CollectEntitiesJob : IJobChunk {

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
            public ComponentDataFromEntity<MeshDataSpan> Spans;

            // Images
            // -----------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<SpriteData> SpriteData;

            [ReadOnly]
            public ComponentDataFromEntity<Stretch> Stretched;

            // Text
            // -----------------------------------------
            [ReadOnly]
            public BufferFromEntity<CharElement> CharBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<DynamicTextTag> DynamicTexts;

            // Parallel Containers
            // -----------------------------------------
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            [WriteOnly]
            public NativeHashMap<Entity, IntPtr>.ParallelWriter CanvasVertexMap;

            [WriteOnly]
            public NativeHashMap<Entity, int2>.ParallelWriter StaticMeshDataMap;

            [NativeDisableUnsafePtrRestriction]
            public UnsafeList<EntityContainer>* ImageContainer;

            [NativeDisableUnsafePtrRestriction]
            public UnsafeList<EntityContainer>* StaticTextContainer;

            [NativeDisableUnsafePtrRestriction]
            public UnsafeList<EntityContainer>* DynamicTextContainer;

#pragma warning disable CS0649
            [NativeSetThreadIndex]
            public int NativeThreadIndex;
#pragma warning restore CS0649

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);
                var vertexBuffers = chunk.GetBufferAccessor(VertexBufferType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity = entities[i];
                    var children = Children[entity].AsNativeArray().AsReadOnly();

                    var index = NativeThreadIndex - 1;

                    UnsafeList<EntityContainer>* img        = ImageContainer + index;
                    UnsafeList<EntityContainer>* staticTxt  = StaticTextContainer + index;
                    UnsafeList<EntityContainer>* dynamicTxt = DynamicTextContainer + index;

                    // Declare x as the vertex span of all static elements
                    // Declare y as the index span of all static elements
                    var spans = new int2();

                    RecurseChildrenDetermineType(children, entity, img, staticTxt, dynamicTxt, ref spans);

                    var vertices = vertexBuffers[i];

                    // Add the vertex ptr so we can write to it.
                    void* vertexPtr = vertices.GetUnsafePtr();
                    CanvasVertexMap.TryAdd(entity, new IntPtr(vertexPtr));

                    // Add the static span.
                    StaticMeshDataMap.TryAdd(entity, spans);

                    CommandBuffer.AddComponent<RebuildMeshTag>(firstEntityIndex + i, entity);
                }
            }

            void RecurseChildrenDetermineType(
                    NativeArray<Child>.ReadOnly children, 
                    Entity root, 
                    UnsafeList<EntityContainer>* imgContainer, 
                    UnsafeList<EntityContainer>* staticTxtContainer,
                    UnsafeList<EntityContainer>* dynamicTxtContainer,
                    ref int2 spans) {

                for (int i = 0; i < children.Length; i++) {
                    var entity = children[i].Value;

                    var hasSpan = Spans.HasComponent(entity);

                    if (SpriteData.HasComponent(entity) && hasSpan) {
                        imgContainer->Add(entity);

                        var span = Spans[entity];
                        spans += new int2(span.VertexSpan.y, span.IndexSpan.y);
                    }

                    if (CharBuffers.HasComponent(entity) && hasSpan) {
                        if (DynamicTexts.HasComponent(entity)) {
                            dynamicTxtContainer->Add(entity);
                        } else {
                            staticTxtContainer->Add(entity);
                            var span = Spans[entity];
                            spans += new int2(span.VertexSpan.y, span.IndexSpan.y);
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
                            ref spans);
                    }
                }
            }
        }

        // NOTE: Assume all static
        [BurstCompile]
        struct BuildImageJob : IJob {

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
        struct BuildStaticTextJob : IJobParallelFor {

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
                var lines          = new NativeList<TextUtil.LineInfo>(10, Allocator.Temp);
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
                    TextUtil.GetVerticalAlignment(heights, fontScale, options.Alignment, extents, totalLineHeight, 
                        lines.Length)) + screenSpace.Translation;

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
        struct BuildDynamicTextJob : IJob {

            public EntityCommandBuffer CommandBuffer;

            public NativeHashMap<Entity, int2> StaticSpans;

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
            public ComponentDataFromEntity<TextOptions> Textoptions;

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

                for (int i = 0; i < minPriorityQueue.Length; i++) {

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
                    var textOptions = Textoptions[textEntity];
                    var color       = AppliedColors[textEntity].Value.ToNormalizedFloat4();

                    var linked   = LinkedTextFont[textEntity].Value;
                    var fontFace = FontFaces[linked];
                    var glyphs   = GlyphBuffers[linked].AsNativeArray();

                    var originalSpan = staticSpan;

                    CreateVertexForChars(
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
                        ref staticSpan);

                    // Update the hashmap with the new spans so the next entities can batch.
                    StaticSpans[rootEntity] = staticSpan;

                    CopyToBuffer(vertices, indices, tempVertices, tempIndices, originalSpan);

                    tempVertices.Clear();
                    tempIndices.Clear();
                    lines.Clear();
                    // TODO: Canvas needs to rebuild itself because of the dynamic elements.
                }

                minPriorityQueue.Dispose();
            }

            void CopyToBuffer(
                DynamicBuffer<Vertex> dstVertices, 
                DynamicBuffer<Index> dstIndices, 
                NativeList<Vertex> srcVertices, 
                NativeList<Index> srcIndices,
                int2 origStaticSpan) {

                var vertexLength = dstVertices.Length - origStaticSpan.x;

                for (int i = 0; i < vertexLength; i++) {
                    dstVertices[i + origStaticSpan.x] = srcVertices[i];
                }

                for (int i = vertexLength; i < srcVertices.Length; i++) {
                    dstVertices.Add(srcVertices[i]);
                }

                var indexLength = dstIndices.Length - origStaticSpan.y;

                for (int i = 0; i < indexLength; i++) {
                    dstIndices[i + origStaticSpan.y] = srcIndices[i];
                }

                for (int i = indexLength; i < srcIndices.Length; i++) {
                    dstIndices.Add(srcIndices[i]);
                }
            }

            void CreateVertexForChars(
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
                    TextUtil.GetVerticalAlignment(heights, fontScale, options.Alignment, extents, totalLineHeight, lines.Length)) + 
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
                CommandBuffer.SetComponent(textEntity, new MeshDataSpan {
                    IndexSpan = new int2(spans.y, indices.Length),
                    VertexSpan = new int2(spans.x, vertices.Length)
                });

                // Update the new offset.
                spans += new int2(vertices.Length, indices.Length);
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

            int threadCount = JobsUtility.JobWorkerCount + 1;

            perThreadImageContainer = new PerThreadContainer<EntityContainer>(
                threadCount,
                20, 
                Allocator.Persistent);

            perThreadDynamicTextContainer = new PerThreadContainer<EntityContainer>(
                threadCount,
                20, 
                Allocator.Persistent);

            perThreadStaticTextContainer = new PerThreadContainer<EntityContainer>(
                threadCount,
                20,
                Allocator.Persistent);
        }

        protected override void OnDestroy() {
            perThreadImageContainer.Dispose();
            perThreadDynamicTextContainer.Dispose();
            perThreadStaticTextContainer.Dispose();
        }

        protected override void OnUpdate() {
            // Clear out all threaded containers
            perThreadImageContainer.Reset();
            perThreadDynamicTextContainer.Reset();
            perThreadStaticTextContainer.Reset();

            var charBuffers = GetBufferFromEntity<CharElement>();
            var spriteData  = GetComponentDataFromEntity<SpriteData>(true);
            var children    = GetBufferFromEntity<Child>(true);

            var dimensions     = GetComponentDataFromEntity<Dimension>(true);
            var colors         = GetComponentDataFromEntity<AppliedColor>(true);
            var spans          = GetComponentDataFromEntity<MeshDataSpan>(true);
            var screenSpaces   = GetComponentDataFromEntity<ScreenSpace>(true);
            var resolutions    = GetComponentDataFromEntity<DefaultSpriteResolution>(true);
            var stretch        = GetComponentDataFromEntity<Stretch>(true);
            var root           = GetComponentDataFromEntity<RootCanvasReference>(true);
            var dynamicTexts   = GetComponentDataFromEntity<DynamicTextTag>(true);
            var submeshIndices = GetComponentDataFromEntity<SubmeshIndex>(true);

            var vertexTypeHandle = GetBufferTypeHandle<Vertex>(false);

            var canvasCount = canvasQuery.CalculateEntityCount();

            var canvasMap         = new NativeHashMap<Entity, IntPtr>(canvasCount, Allocator.TempJob);
            var staticMeshDataMap = new NativeHashMap<Entity, int2>(canvasCount, Allocator.TempJob);
            var commandBuffer     = commandBufferSystem.CreateCommandBuffer();

            // TODO: Collect -> Build Image + Build Text can work here
            var collectDeps          = new CollectEntitiesJob {
                CommandBuffer        = commandBuffer.AsParallelWriter(),
                CharBuffers          = charBuffers,
                Children             = children,
                Stretched            = stretch,
                Spans                = spans,
                EntityType           = GetEntityTypeHandle(),
                SpriteData           = spriteData,
                VertexBufferType     = vertexTypeHandle,
                DynamicTexts         = dynamicTexts,
                ImageContainer       = perThreadImageContainer.Ptr,
                StaticTextContainer  = perThreadStaticTextContainer.Ptr,
                DynamicTextContainer = perThreadDynamicTextContainer.Ptr,
                CanvasVertexMap      = canvasMap.AsParallelWriter(),
                StaticMeshDataMap    = staticMeshDataMap.AsParallelWriter()
            }.ScheduleParallel(canvasQuery, Dependency);

            // Rebuild all the static images
            // ------------------------------------------------------------------------------
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
            }.Schedule(collectDeps);

            var fontFaces   = GetComponentDataFromEntity<FontFaceInfo>(true);
            var glyphs      = GetBufferFromEntity<GlyphElement>(true);
            var linkedFonts = GetComponentDataFromEntity<LinkedTextFontEntity>(true);
            var textOptions = GetComponentDataFromEntity<TextOptions>(true);

            // Rebuild all the static text first
            // ------------------------------------------------------------------------------
            var textDeps        = new BuildStaticTextJob {
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
            }.Schedule(perThreadImageContainer.Length, 1, collectDeps);

            // TODO: Now to organize and create dynamic bins for each canvas we want to add dynamic text to.
            // TODO: Figure out how to have self contained priority queues based on the Canvas entity.

            var sortedEntities = new NativeMultiHashMap<Entity, Entity>(canvasCount, Allocator.TempJob);

            var indices  = GetBufferFromEntity<Index>();
            var vertices = GetBufferFromEntity<Vertex>();

            var combinedDeps = JobHandle.CombineDependencies(imgDeps, textDeps, collectDeps);

            var buildDynamicTextDeps = new BuildDynamicTextJob {
                AppliedColors  = colors,
                CharBuffers    = charBuffers,
                CommandBuffer  = commandBuffer,
                Dimensions     = dimensions,
                DynamicText    = perThreadDynamicTextContainer,
                FontFaces      = fontFaces,
                GlyphBuffers   = glyphs,
                Indices        = indices,
                LinkedTextFont = linkedFonts,
                Roots          = root,
                ScreenSpaces   = screenSpaces,
                StaticSpans    = staticMeshDataMap,
                SubmeshIndices = submeshIndices,
                Textoptions    = textOptions,
                Vertices       = vertices
            }.Schedule(combinedDeps);

            // Clean up the temporary maps
            var canvasPtrMapDisposal  = canvasMap.Dispose(buildDynamicTextDeps);
            var canvasSpanMapDisposal = staticMeshDataMap.Dispose(buildDynamicTextDeps);
            var sortedMapDisposal     = sortedEntities.Dispose(buildDynamicTextDeps);

            Dependency = JobHandle.CombineDependencies(canvasPtrMapDisposal, canvasSpanMapDisposal, sortedMapDisposal);
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

