using TMPro;
using UGUIDOTS.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDOTS.Render.Systems {

    // TODO: Switch to a multithreaded system.
    /**
     * Maybe per canvas, grab all of the Images and Text entities. Build all static content first.
     */
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class BuildRenderHierarchySystem : SystemBase {

        [BurstCompile]
        struct CollectEntitiesJob : IJobChunk {

            public EntityCommandBuffer CommandBuffer;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public EntityTypeHandle EntityType;

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

            // TODO: When parallelizing this, best to have per thread containers.
            [WriteOnly]
            public NativeList<Entity> SimpleImgEntities;

            [WriteOnly]
            public NativeList<Entity> StretchedImgEntities;

            [WriteOnly]
            public NativeList<Entity> TextEntities;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity = entities[i];
                    var children = Children[entity].AsNativeArray().AsReadOnly();

                    RecurseChildrenDetermineType(children, entity);

                    CommandBuffer.AddComponent<RebuildMeshTag>(entity);
                }
            }

            void RecurseChildrenDetermineType(NativeArray<Child>.ReadOnly children, Entity root) {
                for (int i = 0; i < children.Length; i++) {
                    var entity = children[i];
                    if (SpriteData.HasComponent(entity)) {
                        if (Stretched.HasComponent(entity)) {
                            StretchedImgEntities.Add(entity);
                        } else {
                            SimpleImgEntities.Add(entity);
                        }
                    }

                    if (CharBuffers.HasComponent(entity)) {
                        TextEntities.Add(entity);
                    }

                    if (Children.HasComponent(entity)) {
                        var grandChildren = Children[entity].AsNativeArray().AsReadOnly();
                        RecurseChildrenDetermineType(grandChildren, root);
                    }
                }
            }
        }

        // NOTE: Assume all static
        [BurstCompile]
        struct BuildImageJob : IJob {

            public EntityCommandBuffer CommandBuffer;

            public BufferFromEntity<Vertex> VertexData;

            [ReadOnly]
            public NativeList<Entity> Simple;

            [ReadOnly]
            public NativeList<Entity> Stretched;

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

            void UpdateImageVertices(Entity entity, NativeArray<Vertex> tempImageData, bool useRootScale) {
                var root = Root[entity].Value;

                // Get the root data
                var vertices      = VertexData[root];
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

                unsafe {
                    var dst  = (Vertex*)vertices.GetUnsafePtr() + span.VertexSpan.x;
                    var size = UnsafeUtility.SizeOf<Vertex>() * span.VertexSpan.y;
                    UnsafeUtility.MemCpy(dst, tempImageData.GetUnsafePtr(), size);
                }
            }

            public void Execute() {
                var tempImageData = new NativeArray<Vertex>(4, Allocator.Temp);

                // TODO: I think I can just combine both loops into 1
                for (int i = 0; i < Simple.Length; i++) {
                    UpdateImageVertices(Simple[i], tempImageData, true);
                }

                for (int i = 0; i < Stretched.Length; i++) {
                    UpdateImageVertices(Stretched[i], tempImageData, false);
                }
            }
        }

        [BurstCompile]
        struct BuildTextJob : IJob {

            public BufferFromEntity<Vertex> Vertices;

            [ReadOnly]
            public NativeList<Entity> Text;

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

            public void Execute() {
                var lines = new NativeList<TextUtil.LineInfo>(10, Allocator.Temp);
                var vertices = new NativeList<Vertex>(Allocator.Temp);
                for (int i = 0; i < Text.Length; i++) {
                    var entity = Text[i];
                    var linked = LinkedTextFonts[entity].Value;

                    var glyphs   = Glyphs[linked].AsNativeArray();
                    var fontFace = FontFace[linked];

                    var root = Roots[entity].Value;

                    var chars       = CharBuffers[entity].AsNativeArray();
                    var dimension   = Dimensions[entity];
                    var screenSpace = ScreenSpace[entity];
                    var textOptions = TextOptions[entity];
                    var color       = AppliedColors[entity].Value.ToNormalizedFloat4();

                    CreateVertexForChars(
                        chars, 
                        glyphs, 
                        lines, 
                        vertices, 
                        fontFace, 
                        dimension, 
                        screenSpace, 
                        textOptions, 
                        color);

                    var span = Spans[entity];
                    var vertexData = Vertices[root];

                    unsafe {
                        var dst = (Vertex*)vertexData.GetUnsafePtr() + span.VertexSpan.x;
                        var size = span.VertexSpan.y * UnsafeUtility.SizeOf<Vertex>();
                        UnsafeUtility.MemCpy(dst, vertexData.GetUnsafePtr(), size);
                    }

                    vertices.Clear();
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
                TextOptions options,
                float4 color) {

                var isBold          = options.Style == FontStyles.Bold;
                var fontScale       = math.select(1f, options.Size / fontFace.PointSize, options.Size > 0);
                var spaceMultiplier = 1f + math.select(fontFace.NormalStyle.y, fontFace.BoldStyle.y, isBold) * 0.01f;
                var padding         = fontScale * spaceMultiplier;

                TextUtil.CountLines(chars, glyphs, dimension, padding, ref lines);

                var totalLineHeight = lines.Length * fontFace.LineHeight * fontScale * screenSpace.Scale.y;
                var stylePadding = TextUtil.SelectStylePadding(options, fontFace);

                var extents = dimension.Extents() * screenSpace.Scale;
                var height = new float3(fontFace.LineHeight, fontFace.AscentLine, fontFace.DescentLine) * screenSpace.Scale.y;

                var start = new float2(
                    TextUtil.GetHorizontalAlignment(options.Alignment, extents, lines[0].LineWidth * screenSpace.Scale.x),
                    TextUtil.GetVerticalAlignment(height, fontScale, options.Alignment, extents, totalLineHeight, lines.Length)
                ) + screenSpace.Translation;

                for (int i = 0, row = 0; i < chars.Length; i++) {
                    var c = chars[i];
                    // TODO: Generate the actual vertices for all the elements.
                    if (!FindGlyphWithChar(glyphs, c, out GlyphElement glyph)) {
                        continue;
                    }

                    if (row < lines.Length && i == lines[row].StartIndex) {
                        var heightOffset = fontFace.LineHeight * fontScale * screenSpace.Scale.y * (row > 0 ? 1f : 0f);
                        start.y -= heightOffset;
                        start.x -= TextUtil.GetHorizontalAlignment(
                            options.Alignment, 
                            extents, 
                            lines[row].LineWidth * screenSpace.Scale.x) + screenSpace.Translation.x;
                        row++;
                    }

                    var xPos = start.x + (glyph.Bearings.x - stylePadding) * fontScale * screenSpace.Scale.x;
                    var yPos = start.y - (glyph.Size.y - glyph.Bearings.y - stylePadding) * fontScale * screenSpace.Scale.y;
                    var size = (glyph.Size + new float2(stylePadding * 2)) * fontScale * screenSpace.Scale;
                    var uv1  = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);

                    var canvasScale = screenSpace.Scale.x / 4 * 3;
                    var uv2         = new float2(glyph.Scale) * math.select(canvasScale, -canvasScale, isBold);
                    var normal      = new float3(1, 0, 0);

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

                    start.x += glyph.Advance * padding * screenSpace.Scale.x;
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

        private EntityQuery canvasQuery;
        private EntityQuery imageQuery;
        private EntityQuery textQuery;
        private EntityCommandBufferSystem commandBufferSystem;

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
        }

        // TODO: Put this on separate threads.
        protected override void OnUpdate() {
            var charBuffers = GetBufferFromEntity<CharElement>();
            var spriteData  = GetComponentDataFromEntity<SpriteData>(true);
            var children    = GetBufferFromEntity<Child>(true);

            var images = new NativeList<Entity>(
                imageQuery.CalculateEntityCountWithoutFiltering(), 
                Allocator.TempJob);

            var stretched = new NativeList<Entity>(
                imageQuery.CalculateChunkCountWithoutFiltering(),
                Allocator.TempJob);

            var texts  = new NativeList<Entity>(
                textQuery.CalculateChunkCountWithoutFiltering(), 
                Allocator.TempJob);

            var dimensions    = GetComponentDataFromEntity<Dimension>(true);
            var colors        = GetComponentDataFromEntity<AppliedColor>(true);
            var spans         = GetComponentDataFromEntity<MeshDataSpan>(true);
            var screenSpaces  = GetComponentDataFromEntity<ScreenSpace>(true);
            var resolutions   = GetComponentDataFromEntity<DefaultSpriteResolution>(true);
            var stretch       = GetComponentDataFromEntity<Stretch>(true);
            var root          = GetComponentDataFromEntity<RootCanvasReference>(true);
            var vertexBuffers = GetBufferFromEntity<Vertex>(false);

            var commandBuffer = commandBufferSystem.CreateCommandBuffer();

            var collectJob           = new CollectEntitiesJob {
                CommandBuffer        = commandBufferSystem.CreateCommandBuffer(),
                CharBuffers          = charBuffers,
                Children             = children,
                Stretched            = stretch,
                EntityType           = GetEntityTypeHandle(),
                SpriteData           = spriteData,
                SimpleImgEntities    = images,
                StretchedImgEntities = stretched,
                TextEntities         = texts
            };

            collectJob.Run(canvasQuery);

            var imageJob          = new BuildImageJob {
                Root              = root,
                Colors            = colors,
                Dimensions        = dimensions,
                MeshDataSpans     = spans,
                ScreenSpaces      = screenSpaces,
                SpriteData        = spriteData,
                SpriteResolutions = resolutions,
                VertexData        = vertexBuffers,
                Simple            = images,
                Stretched         = stretched,
                CommandBuffer     = commandBuffer
            };

            imageJob.Run();

            var fontFaces   = GetComponentDataFromEntity<FontFaceInfo>(true);
            var glyphs      = GetBufferFromEntity<GlyphElement>(true);
            var linkedFonts = GetComponentDataFromEntity<LinkedTextFontEntity>(true);
            var textOptions = GetComponentDataFromEntity<TextOptions>(true);

            var textJob         = new BuildTextJob {
                AppliedColors   = colors,
                CharBuffers     = charBuffers,
                Dimensions      = dimensions,
                FontFace        = fontFaces,
                Glyphs          = glyphs,
                LinkedTextFonts = linkedFonts,
                ScreenSpace     = screenSpaces,
                Spans           = spans,
                Text            = texts,
                TextOptions     = textOptions,
                Roots           = root,
                Vertices        = vertexBuffers,
            };

            textJob.Run();

            // Dispose all the temp containers
            // --------------------------
            images.Dispose();
            stretched.Dispose();
            texts.Dispose();

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
