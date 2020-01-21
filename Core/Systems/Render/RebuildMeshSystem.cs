using UGUIDots.Conversions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RebuildMeshSystem : JobComponentSystem {

        private struct GenerateMeshJob : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public ArchetypeChunkBufferType<MeshVertexData> VertexType;

            [ReadOnly]
            public ArchetypeChunkComponentType<Dimensions> DimensionType;

            public ArchetypeChunkBufferType<MeshVertexData> VertexAccessorType;
            public ArchetypeChunkBufferType<TriangleIndexElement> TriangleIdxType;
            
            [ReadOnly]
            public EntityManager Manager;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities       = chunk.GetNativeArray(EntityType);
                var dimensions     = chunk.GetNativeArray(DimensionType);
                var vertexAccessor = chunk.GetBufferAccessor(VertexAccessorType);
                var indexAccessor  = chunk.GetBufferAccessor(TriangleIdxType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity = entities[i];
                    var dimension = dimensions[i];

                    var vertices = vertexAccessor[i];
                    vertices.Clear();

                    vertices.Add(new MeshVertexData {
                    });

                    var indices = indexAccessor[i];
                    indices.Clear();
                }
            }

            private void BuildImageMeshData(ref DynamicBuffer<MeshVertexData> vertices, 
                ref DynamicBuffer<TriangleIndexElement> indices, in float2 size) {

                vertices.Clear();
                indices.Clear();

                vertices.Add(new MeshVertexData {
                    
                });
                vertices.Add(new MeshVertexData { });
                vertices.Add(new MeshVertexData { });
                vertices.Add(new MeshVertexData { });
            }
        }

        private EntityQuery imageQuery;

        protected override void OnCreate() {
            imageQuery = GetEntityQuery(new EntityQueryDesc {
                All  = new[] { ComponentType.ReadOnly<Dimensions>(), ComponentType.ReadOnly<MeshRebuildTag>() },
                Any  = new[] { ComponentType.ReadOnly<DynamicRenderTag>() },
                None = new[] { ComponentType.ReadOnly<CharElement>() }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
