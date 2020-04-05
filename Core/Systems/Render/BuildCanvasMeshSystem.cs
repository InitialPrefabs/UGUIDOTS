using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup)), UpdateAfter(typeof(BatchCanvasVertexSystem))]
    public class BuildCanvasMeshSystem : SystemBase {

        private EntityQuery canvasMeshQuery;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            canvasMeshQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<RootVertexData>(), ComponentType.ReadOnly<RootTriangleIndexElement>(),
                    ComponentType.ReadOnly<SubmeshSliceElement>(), ComponentType.ReadOnly<BuildCanvasTag>()
                }
            });

            commandBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            RequireForUpdate(canvasMeshQuery);
        }

        protected override void OnUpdate() {
            var cmdBuffer = commandBufferSystem.CreateCommandBuffer();

            Entities.WithStoreEntityQueryInField(ref canvasMeshQuery).WithoutBurst().ForEach((
                Entity entity,
                Mesh mesh,
                DynamicBuffer<RootVertexData> vertices,
                DynamicBuffer<RootTriangleIndexElement> indices,
                DynamicBuffer<SubmeshSliceElement> submeshDesc) => {

                mesh.Clear();
                mesh.SetVertexBufferParams(vertices.Length, MeshVertexDataExtensions.VertexDescriptors);
                mesh.SetVertexBufferData(vertices.AsNativeArray(), 0, 0, vertices.Length);
                mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);
                mesh.SetIndexBufferData(indices.AsNativeArray(), 0, 0, indices.Length);

                mesh.subMeshCount = submeshDesc.Length;

                for (int i = 0; i < submeshDesc.Length; i++) {
                    var current = submeshDesc[i];

                    mesh.SetSubMesh(i, new SubMeshDescriptor {
                        bounds      = default,
                        indexStart  = current.IndexSpan.x,
                        indexCount  = current.IndexSpan.y,
                        firstVertex = current.VertexSpan.x,
                        vertexCount = current.VertexSpan.y,
                        topology    = MeshTopology.Triangles,
                    });
                }

                mesh.UploadMeshData(false);
                cmdBuffer.RemoveComponent<BuildCanvasTag>(entity);
            }).Run();
        }
    }
}
