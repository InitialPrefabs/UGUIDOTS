using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchingGroup))]
    public class BuildImageVertexDataSystem : JobComponentSystem {

        [BurstCompile]
        private unsafe struct RebuildImgMeshJob : IJobChunk {

            public ProfilerMarker BuildMeshProfiler;

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

            public ArchetypeChunkBufferType<MeshVertexData> VertexType;
            public ArchetypeChunkBufferType<TriangleIndexElement> TriangleType;

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                BuildMeshProfiler.Begin();

                var dimensions     = chunk.GetNativeArray(DimensionType);
                var vertexBuffer   = chunk.GetBufferAccessor(VertexType);
                var triangleBuffer = chunk.GetBufferAccessor(TriangleType);
                var colors         = chunk.GetNativeArray(ColorType);
                var entities       = chunk.GetNativeArray(EntityType);
                var spriteData     = chunk.GetNativeArray(SpriteDataType);

                for (int i = 0; i < chunk.Count; i++) {
                    var dimension = dimensions[i];
                    var indices   = triangleBuffer[i];
                    var vertices  = vertexBuffer[i];
                    var color     = colors[i].Value.ToNormalizedFloat4();
                    var entity    = entities[i];
                    var uv        = spriteData[i].OuterUV;

                    indices.Clear();
                    vertices.Clear();

                    var right   = new float3(1, 0, 0);
                    var extents = dimension.Extents();

                    vertices.Add(new MeshVertexData {
                        Position = new float3(-extents.x, -extents.y, 0),
                        Normal   = right,
                        Color    = color,
                        UV1       = new float2(uv.x, uv.y),
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(-extents.x, extents.y, 0),
                        Normal   = right,
                        Color    = color,
                        UV1       = new float2(uv.x, uv.w),
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(extents, 0),
                        Normal   = right,
                        Color    = color,
                        UV1       = new float2(uv.z, uv.w),
                    });
                    vertices.Add(new MeshVertexData {
                        Position = new float3(extents.x, -extents.y, 0),
                        Normal   = right,
                        Color    = color,
                        UV1       = new float2(uv.z, uv.y),
                    });

                    // TODO: Figure this out mathematically instead of hard coding
                    // batched meshes need to be figured out and rebuilt...
                    indices.Add(new TriangleIndexElement { Value = 0 });
                    indices.Add(new TriangleIndexElement { Value = 1 });
                    indices.Add(new TriangleIndexElement { Value = 2 });
                    indices.Add(new TriangleIndexElement { Value = 0 });
                    indices.Add(new TriangleIndexElement { Value = 2 });
                    indices.Add(new TriangleIndexElement { Value = 3 });

                    CmdBuffer.AddComponent<CachedMeshTag>(entity.Index, entity);
                }
                BuildMeshProfiler.End();
            }
        }

        private EntityQuery graphicQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            graphicQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<Dimensions>(), ComponentType.ReadWrite<MeshVertexData>(),
                    ComponentType.ReadWrite<TriangleIndexElement>()
                },
                None = new [] {
                    ComponentType.ReadOnly<CachedMeshTag>(), ComponentType.ReadOnly<CharElement>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var rebuildDeps = new RebuildImgMeshJob {
                BuildMeshProfiler = new ProfilerMarker("BuildImageVertexDataSystem.RebuildImgMeshJob"),
                DimensionType     = GetArchetypeChunkComponentType<Dimensions>(true),
                ColorType         = GetArchetypeChunkComponentType<AppliedColor>(true),
                VertexType        = GetArchetypeChunkBufferType<MeshVertexData>(),
                TriangleType      = GetArchetypeChunkBufferType<TriangleIndexElement>(),
                CharType          = GetArchetypeChunkBufferType<CharElement>(true),
                SpriteDataType    = GetArchetypeChunkComponentType<SpriteData>(true),
                EntityType        = GetArchetypeChunkEntityType(),
                CmdBuffer         = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(graphicQuery, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(rebuildDeps);
            return rebuildDeps;
        }
    }
}
