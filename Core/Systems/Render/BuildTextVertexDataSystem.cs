using System.Runtime.CompilerServices;
using TMPro;
using UGUIDots.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBuildGroup))]
    public class BuildTextVertexDataSystem : SystemBase {

        [BurstCompile]
        private struct BuildGlyphMapJobChunk : IJobChunk {

            [WriteOnly] public NativeHashMap<int, Entity>.ParallelWriter GlyphMap;
            [ReadOnly] public ArchetypeChunkComponentType<FontID> FontType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var fonts = chunk.GetNativeArray(FontType);
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    GlyphMap.TryAdd(fonts[i].Value, entities[i]);
                }
            }
        }

        [BurstCompile]
        private struct BuildTextMeshJob : IJobChunk {

            [ReadOnly] public NativeHashMap<int, Entity> GlyphMap;
            [ReadOnly] public BufferFromEntity<GlyphElement> GlyphData;
            [ReadOnly] public ComponentDataFromEntity<FontFaceInfo> FontFaces;
            [ReadOnly] public ComponentDataFromEntity<Parent> Parents;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkBufferType<CharElement> CharBufferType;
            [ReadOnly] public ArchetypeChunkComponentType<TextOptions> TextOptionType;
            [ReadOnly] public ArchetypeChunkComponentType<TextFontID> TxtFontIDType;
            [ReadOnly] public ArchetypeChunkComponentType<AppliedColor> ColorType;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> LTWType;
            [ReadOnly] public ArchetypeChunkComponentType<Dimensions> DimensionType;

            public ArchetypeChunkBufferType<LocalVertexData> MeshVertexDataType;
            public ArchetypeChunkBufferType<LocalTriangleIndexElement> TriangleIndexType;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var textBufferAccessor    = chunk.GetBufferAccessor(CharBufferType);
                var vertexBufferAccessor  = chunk.GetBufferAccessor(MeshVertexDataType);
                var triangleIndexAccessor = chunk.GetBufferAccessor(TriangleIndexType);
                var textOptions           = chunk.GetNativeArray(TextOptionType);
                var txtFontIDs            = chunk.GetNativeArray(TxtFontIDType);
                var entities              = chunk.GetNativeArray(EntityType);
                var colors                = chunk.GetNativeArray(ColorType);
                var ltws                  = chunk.GetNativeArray(LTWType);
                var dimensionsChunk       = chunk.GetNativeArray(DimensionType);

                for (int i         = 0; i < chunk.Count; i++) {
                    var text       = textBufferAccessor[i].AsNativeArray();
                    var vertices   = vertexBufferAccessor[i];
                    var indices    = triangleIndexAccessor[i];
                    var fontID     = txtFontIDs[i].Value;
                    var textOption = textOptions[i];
                    var color      = colors[i];
                    var dimensions = dimensionsChunk[i];
                    var ltw        = ltws[i];

                    vertices.Clear();
                    indices .Clear();

                    var canvasScale = ltw.AverageScale();

                    var glyphTableExists  = GlyphMap.TryGetValue(fontID, out var glyphEntity);
                    var glyphBufferExists = GlyphData.Exists(glyphEntity);
                    var fontFaceExists    = FontFaces.Exists(glyphEntity);

                    if (!(glyphTableExists && glyphBufferExists && fontFaceExists)) {
                        continue;
                    }

                    var fontFace  = FontFaces[glyphEntity];
                    var fontScale = textOption.Size > 0 ? ((float)textOption.Size / fontFace.PointSize) : 1f;
                    var glyphData = GlyphData[glyphEntity].AsNativeArray();
                    var extents   = dimensions.Extents() * ltw.Scale().xy;

                    var stylePadding = TextUtil.SelectStylePadding(in textOption, in fontFace);
                    var parentScale  = textOption.Size * new float2(1) / fontFace.PointSize;
                    var isBold       = textOption.Style == FontStyles.Bold;

                    var styleSpaceMultiplier = 1f + (isBold ? fontFace.BoldStyle.y * 0.01f : fontFace.NormalStyle.y * 0.01f);
                    var padding = fontScale * styleSpaceMultiplier;
                    var lines = new NativeList<TextUtil.LineInfo>(Allocator.Temp);

                    TextUtil.CountLines(in text, in glyphData, dimensions, padding, ref lines);

                    var linesHeight = lines.Length * fontFace.LineHeight * fontScale * ltw.Scale().y;
                    var heights     = new float3(fontFace.LineHeight, fontFace.AscentLine, fontFace.DescentLine) * ltw.Scale().y;

                    var start = new float2(
                        TextUtil.GetHorizontalAlignment(textOption.Alignment, extents, lines[0].LineWidth * ltw.Scale().x),
                        TextUtil.GetVerticalAlignment(heights, fontScale, textOption.Alignment,
                            in extents, in linesHeight, lines.Length)) + ltw.Position.xy;

                    for (int k = 0, row = 0; k < text.Length; k++) {
                        var c = text[k].Value;

                        if (!glyphData.TryGetGlyph(c, out var glyph)) {
                            continue;
                        }

                        var bl = (ushort)vertices.Length;

                        if (row < lines.Length && k == lines[row].StartIndex) {
                            var height = fontFace.LineHeight * fontScale * ltw.Scale().y * (row > 0 ? 1f : 0f);

                            start.y -= height;
                            start.x  = TextUtil.GetHorizontalAlignment(textOption.Alignment,
                                    extents, lines[row].LineWidth * ltw.Scale().x) + ltw.Position.x;

                            row++;
                        }

                        var xPos = start.x + (glyph.Bearings.x - stylePadding) * fontScale * ltw.Scale().x;
                        var yPos = start.y - (glyph.Size.y - glyph.Bearings.y - stylePadding) * fontScale * ltw.Scale().y;

                        var size  = (glyph.Size + new float2(stylePadding * 2)) * fontScale * ltw.Scale().xy;
                        var uv1   = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);
                        var uv2   = new float2(glyph.Scale) * math.select(canvasScale, -canvasScale, isBold);
                        var right = new float3(1, 0, 0);

                        var vertexColor = color.Value.ToNormalizedFloat4();

                        vertices.Add(new LocalVertexData {
                            Position = new float3(xPos, yPos, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c0,
                            UV2      = uv2
                        });
                        vertices.Add(new LocalVertexData {
                            Position = new float3(xPos, yPos + size.y, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c1,
                            UV2      = uv2
                        });
                        vertices.Add(new LocalVertexData {
                            Position = new float3(xPos + size.x, yPos + size.y, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c2,
                            UV2      = uv2
                        });
                        vertices.Add(new LocalVertexData {
                            Position = new float3(xPos + size.x, yPos, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c3,
                            UV2      = uv2
                        });

                        var tl = (ushort)(bl + 1);
                        var tr = (ushort)(bl + 2);
                        var br = (ushort)(bl + 3);

                        indices.Add(new LocalTriangleIndexElement { Value = bl });
                        indices.Add(new LocalTriangleIndexElement { Value = tl });
                        indices.Add(new LocalTriangleIndexElement { Value = tr });

                        indices.Add(new LocalTriangleIndexElement { Value = bl });
                        indices.Add(new LocalTriangleIndexElement { Value = tr });
                        indices.Add(new LocalTriangleIndexElement { Value = br });

                        start += new float2(glyph.Advance * padding, 0) * ltw.Scale().xy;
                    }

                    lines.Dispose();
                    var textEntity = entities[i];

                    CmdBuffer.RemoveComponent<BuildUIElementTag>(textEntity.Index, textEntity);

                    var canvas = GetRootCanvas(textEntity);
                    CmdBuffer.AddComponent(canvas.Index, canvas, new BatchCanvasTag { });
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Entity GetRootCanvas(Entity current) {
                if (Parents.Exists(current)) {
                    return GetRootCanvas(Parents[current].Value);
                }
                return current;
            }
        }

        private EntityQuery glyphQuery, textQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            glyphQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<GlyphElement>(),
                    ComponentType.ReadOnly<FontID>()
                }
            });

            textQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadWrite<LocalVertexData>(),
                    ComponentType.ReadWrite<LocalTriangleIndexElement>(),
                    ComponentType.ReadOnly<CharElement>(),
                    ComponentType.ReadOnly<TextOptions>(),
                    ComponentType.ReadOnly<BuildUIElementTag>()
                }
            });

            RequireForUpdate(textQuery);

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var glyphMap = new NativeHashMap<int, Entity>(glyphQuery.CalculateEntityCount(), Allocator.TempJob);

            Dependency = new BuildGlyphMapJobChunk {
                GlyphMap   = glyphMap.AsParallelWriter(),
                EntityType = GetArchetypeChunkEntityType(),
                FontType   = GetArchetypeChunkComponentType<FontID>(true)
            }.Schedule(glyphQuery, Dependency);

            Dependency = new BuildTextMeshJob {
                GlyphMap           = glyphMap,
                GlyphData          = GetBufferFromEntity<GlyphElement>(true),
                FontFaces          = GetComponentDataFromEntity<FontFaceInfo>(true),
                Parents            = GetComponentDataFromEntity<Parent>(true),
                EntityType         = GetArchetypeChunkEntityType(),
                CharBufferType     = GetArchetypeChunkBufferType<CharElement>(true),
                TextOptionType     = GetArchetypeChunkComponentType<TextOptions>(true),
                TxtFontIDType      = GetArchetypeChunkComponentType<TextFontID>(true),
                ColorType          = GetArchetypeChunkComponentType<AppliedColor>(true),
                LTWType            = GetArchetypeChunkComponentType<LocalToWorld>(true),
                DimensionType      = GetArchetypeChunkComponentType<Dimensions>(true),
                MeshVertexDataType = GetArchetypeChunkBufferType<LocalVertexData>(),
                TriangleIndexType  = GetArchetypeChunkBufferType<LocalTriangleIndexElement>(),
                CmdBuffer          = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(textQuery, Dependency);

            Dependency = glyphMap.Dispose(Dependency);
            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
