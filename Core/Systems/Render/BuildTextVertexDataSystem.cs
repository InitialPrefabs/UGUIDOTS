using TMPro;
using UGUIDots.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchingGroup))]
    public class BuildTextVertexDataSystem : JobComponentSystem {

        [BurstCompile]
        private struct BuildGlyphMapJob : IJobForEachWithEntity<FontID> {

            [WriteOnly]
            public NativeHashMap<int, Entity>.ParallelWriter GlyphMap;

            public void Execute(Entity entity, int index, ref FontID c0) {
                GlyphMap.TryAdd(c0.Value, entity);
            }
        }

        [BurstCompile]
        private struct BuildTextMeshJob : IJobChunk {

            [ReadOnly] public NativeHashMap<int, Entity> GlyphMap;
            [ReadOnly] public BufferFromEntity<GlyphElement> GlyphData;
            [ReadOnly] public ComponentDataFromEntity<FontFaceInfo> FontFaces;

            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkBufferType<CharElement> CharBufferType;
            [ReadOnly] public ArchetypeChunkComponentType<TextOptions> TextOptionType;
            [ReadOnly] public ArchetypeChunkComponentType<TextFontID> TxtFontIDType;
            [ReadOnly] public ArchetypeChunkComponentType<AppliedColor> ColorType;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> LTWType;
            [ReadOnly] public ArchetypeChunkComponentType<Dimensions> DimensionType;

            public ArchetypeChunkBufferType<MeshVertexData> MeshVertexDataType;
            public ArchetypeChunkBufferType<TriangleIndexElement> TriangleIndexType;

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
                    var fontScale = textOption.Size > 0 ? (float)textOption.Size / fontFace.PointSize : 1f;
                    var glyphData = GlyphData[glyphEntity].AsNativeArray();
                    var extents   = dimensions.Extents();

                    var stylePadding = TextUtil.SelectStylePadding(in textOption, in fontFace);
                    var parentScale  = textOption.Size * new float2(1) / fontFace.PointSize;
                    var isBold       = textOption.Style == FontStyles.Bold;

                    var styleSpaceMultiplier = 1f + (isBold ? fontFace.BoldStyle.y : fontFace.NormalStyle.y) * 0.01f;
                    var padding = fontScale * styleSpaceMultiplier;
                    var lines = new NativeList<TextUtil.LineInfo>(Allocator.Temp);

                    TextUtil.CountLines(in text, in glyphData, dimensions, padding, ref lines);

                    var linesHeight = lines.Length * fontFace.LineHeight * fontScale;
                    var heights     = new float3(fontFace.LineHeight, fontFace.AscentLine, fontFace.DescentLine);

                    var start = new float2(
                        TextUtil.GetHorizontalAlignment(textOption.Alignment, extents, lines[0].LineWidth),
                        TextUtil.GetVerticalAlignment(heights, fontScale, textOption.Alignment, 
                            in extents, in linesHeight, lines.Length));

                    for (int k = 0, row = 0; k < text.Length; k++) {
                        var c = text[k].Value;

                        if (!glyphData.TryGetGlyph(c, out var glyph)) {
                            continue;
                        }

                        var bl = (ushort)vertices.Length;

                        if (row < lines.Length && k == lines[row].StartIndex) {
                            var height = fontFace.LineHeight * fontScale * (row > 0 ? 1f : 0f);

                            start.y -= height;
                            start.x  = TextUtil.GetHorizontalAlignment(textOption.Alignment, 
                                    extents, lines[row].LineWidth);

                            row++;
                        }

                        var xPos = start.x + (glyph.Bearings.x - stylePadding) * fontScale;
                        var yPos = start.y - (glyph.Size.y - glyph.Bearings.y - stylePadding) * fontScale;

                        var size  = (glyph.Size + new float2(stylePadding * 2)) * fontScale;
                        var uv1   = glyph.RawUV.NormalizeAdjustedUV(stylePadding, fontFace.AtlasSize);
                        var uv2   = new float2(glyph.Scale) * math.select(canvasScale, -canvasScale, isBold);
                        var right = new float3(1, 0, 0);

                        var vertexColor = color.Value.ToNormalizedFloat4();

                        vertices.Add(new MeshVertexData {
                            Position = new float3(xPos, yPos, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c0,
                            UV2      = uv2
                        });
                        vertices.Add(new MeshVertexData {
                            Position = new float3(xPos, yPos + size.y, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c1,
                            UV2      = uv2
                        });
                        vertices.Add(new MeshVertexData {
                            Position = new float3(xPos + size.x, yPos + size.y, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c2,
                            UV2      = uv2
                        });
                        vertices.Add(new MeshVertexData {
                            Position = new float3(xPos + size.x, yPos, 0),
                            Normal   = right,
                            Color    = vertexColor,
                            UV1      = uv1.c3,
                            UV2      = uv2
                        });

                        var tl = (ushort)(bl + 1);
                        var tr = (ushort)(bl + 2);
                        var br = (ushort)(bl + 3);

                        indices.Add(new TriangleIndexElement { Value = bl });
                        indices.Add(new TriangleIndexElement { Value = tl });
                        indices.Add(new TriangleIndexElement { Value = tr });

                        indices.Add(new TriangleIndexElement { Value = bl });
                        indices.Add(new TriangleIndexElement { Value = tr });
                        indices.Add(new TriangleIndexElement { Value = br });

                        start += new float2(glyph.Advance * padding, 0);
                    }

                    lines.Dispose();
                    var textEntity = entities[i];
                    CmdBuffer.RemoveComponent<BuildTextTag>(textEntity.Index, textEntity);
                }

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
                    ComponentType.ReadWrite<MeshVertexData>(),
                    ComponentType.ReadWrite<TriangleIndexElement>(),
                    ComponentType.ReadOnly<CharElement>(),
                    ComponentType.ReadOnly<TextOptions>(),
                    ComponentType.ReadOnly<BuildTextTag>()
                }
            });

            RequireForUpdate(textQuery);

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var glyphMap = new NativeHashMap<int, Entity>(glyphQuery.CalculateEntityCount(), Allocator.TempJob);

            var glyphMapDeps = new BuildGlyphMapJob {
                GlyphMap = glyphMap.AsParallelWriter()
            }.Schedule(glyphQuery, inputDeps);

            var textMeshDeps       = new BuildTextMeshJob {
                GlyphMap           = glyphMap,
                GlyphData          = GetBufferFromEntity<GlyphElement>(true),
                FontFaces          = GetComponentDataFromEntity<FontFaceInfo>(true),
                EntityType         = GetArchetypeChunkEntityType(),
                CharBufferType     = GetArchetypeChunkBufferType<CharElement>(true),
                TextOptionType     = GetArchetypeChunkComponentType<TextOptions>(true),
                TxtFontIDType      = GetArchetypeChunkComponentType<TextFontID>(true),
                ColorType          = GetArchetypeChunkComponentType<AppliedColor>(true),
                LTWType            = GetArchetypeChunkComponentType<LocalToWorld>(true),
                DimensionType      = GetArchetypeChunkComponentType<Dimensions>(true),
                MeshVertexDataType = GetArchetypeChunkBufferType<MeshVertexData>(),
                TriangleIndexType  = GetArchetypeChunkBufferType<TriangleIndexElement>(),
                CmdBuffer          = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(textQuery, glyphMapDeps);

            var finalDeps = glyphMap.Dispose(textMeshDeps);

            cmdBufferSystem.AddJobHandleForProducer(finalDeps);

            return finalDeps;
        }
    }
}
