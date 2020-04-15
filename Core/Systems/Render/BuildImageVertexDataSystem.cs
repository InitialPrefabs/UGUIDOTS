using System.Runtime.CompilerServices;
using UGUIDots.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBuildGroup))]
    public class BuildImageVertexDataSystem : SystemBase {

        [BurstCompile]
        private unsafe struct RebuildImgMeshJob : IJobChunk {

            public ProfilerMarker BuildMeshProfiler;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public ArchetypeChunkComponentType<LocalToWorld> LTWType;

            [ReadOnly]
            public ArchetypeChunkComponentType<Dimensions> DimensionType;

            [ReadOnly]
            public ArchetypeChunkBufferType<CharElement> CharType;

            [ReadOnly]
            public ArchetypeChunkComponentType<AppliedColor> ColorType;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public ArchetypeChunkComponentType<SpriteData> SpriteDataType;

            [ReadOnly]
            public ArchetypeChunkComponentType<DefaultSpriteResolution> SpriteResType;

            public ArchetypeChunkBufferType<LocalVertexData> VertexType;
            public ArchetypeChunkBufferType<LocalTriangleIndexElement> TriangleType;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                BuildMeshProfiler.Begin();

                var dimensions     = chunk.GetNativeArray(DimensionType);
                var vertexBuffer   = chunk.GetBufferAccessor(VertexType);
                var triangleBuffer = chunk.GetBufferAccessor(TriangleType);
                var colors         = chunk.GetNativeArray(ColorType);
                var entities       = chunk.GetNativeArray(EntityType);
                var spriteData     = chunk.GetNativeArray(SpriteDataType);
                var resolutions    = chunk.GetNativeArray(SpriteResType);
                var ltws           = chunk.GetNativeArray(LTWType);

                for (int i         = 0; i < chunk.Count; i++) {
                    var dimension  = dimensions[i];
                    var indices    = triangleBuffer[i];
                    var vertices   = vertexBuffer[i];
                    var color      = colors[i].Value.ToNormalizedFloat4();
                    var imgEntity  = entities[i];
                    var spriteInfo = spriteData[i];
                    var resolution = resolutions[i].Value;
                    var position   = ltws[i].Position;
                    var scale      = ltws[i].Scale();

                    var spriteScale = (float2)(dimension.Value) / resolution;

                    indices.Clear();
                    vertices.Clear();

                    var right   = new float3(1, 0, 0);
                    var extents = dimension.Extents();

                    var outer   = spriteInfo.OuterUV;
                    var padding = spriteInfo.Padding;

                    var bl = position.xy - extents * scale.xy;

                    var spriteW = dimension.Width();
                    var spriteH = dimension.Height();

                    var pixelAdjustments = new float4(
                        (padding.x * spriteScale.x) / spriteW,
                        (padding.y * spriteScale.y) / spriteH,
                        (spriteW - padding.z * spriteScale.x) / spriteW,
                        (spriteH - padding.w * spriteScale.y) / spriteH
                    );

                    var pixelYAdjust = spriteScale.y * 1.5f;
                    var topAdjust    = spriteScale.y * (padding.w > 0 ? 1f : 0f);
                    var bottomAdjust = spriteScale.y * (padding.y > 0 ? 1f : 0f);

                    var v = new float4(
                        bl.x + spriteW * pixelAdjustments.x * scale.x,
                        (bl.y + spriteH * pixelAdjustments.y * scale.y) + bottomAdjust,
                        bl.x + spriteW * pixelAdjustments.z * scale.x,
                        (bl.y + spriteH * pixelAdjustments.w * scale.y) - topAdjust
                    );

                    vertices.Add(new LocalVertexData {
                        Position = new float3(v.xy, 0),
                        Normal   = right,
                        Color    = color,
                        UV1      = outer.xy,
                        UV2      = new float2(1)
                    });
                    vertices.Add(new LocalVertexData {
                        Position = new float3(v.xw, 0),
                        Normal   = right,
                        Color    = color,
                        UV1      = outer.xw,
                        UV2      = new float2(1)
                    });
                    vertices.Add(new LocalVertexData {
                        Position = new float3(v.zw, 0),
                        Normal   = right,
                        Color    = color,
                        UV1      = outer.zw,
                        UV2      = new float2(1)
                    });
                    vertices.Add(new LocalVertexData {
                        Position = new float3(v.zy, 0),
                        Normal   = right,
                        Color    = color,
                        UV1      = outer.zy,
                        UV2      = new float2(1)
                    });

                    // TODO: Figure this out mathematically instead of hard coding
                    // batched meshes need to be figured out and rebuilt...
                    indices.Add(new LocalTriangleIndexElement { Value = 0 });
                    indices.Add(new LocalTriangleIndexElement { Value = 1 });
                    indices.Add(new LocalTriangleIndexElement { Value = 2 });
                    indices.Add(new LocalTriangleIndexElement { Value = 0 });
                    indices.Add(new LocalTriangleIndexElement { Value = 2 });
                    indices.Add(new LocalTriangleIndexElement { Value = 3 });

                    CmdBuffer.RemoveComponent<BuildUIElementTag>(imgEntity.Index, imgEntity);

                    // Signal that the canvas has to be built.
                    var canvas = GetRootCanvas(imgEntity);
                    CmdBuffer.AddComponent(canvas.Index, canvas, new BatchCanvasTag { });
                }
                BuildMeshProfiler.End();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Entity GetRootCanvas(Entity current) {
                if (Parents.Exists(current)) {
                    return GetRootCanvas(Parents[current].Value);
                }

                return current;
            }
        }

        private EntityQuery graphicQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            graphicQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<Dimensions>(), ComponentType.ReadWrite<LocalVertexData>(),
                    ComponentType.ReadWrite<LocalTriangleIndexElement>(), ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ReadOnly<BuildUIElementTag>()
                },
                None = new [] {
                    ComponentType.ReadOnly<CharElement>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            Dependency            = new RebuildImgMeshJob {
                BuildMeshProfiler = new ProfilerMarker("BuildImageVertexDataSystem.RebuildImgMeshJob"),
                Parents           = GetComponentDataFromEntity<Parent>(true),
                LTWType           = GetArchetypeChunkComponentType<LocalToWorld>(true),
                DimensionType     = GetArchetypeChunkComponentType<Dimensions>(true),
                ColorType         = GetArchetypeChunkComponentType<AppliedColor>(true),
                VertexType        = GetArchetypeChunkBufferType<LocalVertexData>(),
                TriangleType      = GetArchetypeChunkBufferType<LocalTriangleIndexElement>(),
                CharType          = GetArchetypeChunkBufferType<CharElement>(true),
                SpriteDataType    = GetArchetypeChunkComponentType<SpriteData>(true),
                SpriteResType     = GetArchetypeChunkComponentType<DefaultSpriteResolution>(true),
                EntityType        = GetArchetypeChunkEntityType(),
                CmdBuffer         = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(graphicQuery, Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
