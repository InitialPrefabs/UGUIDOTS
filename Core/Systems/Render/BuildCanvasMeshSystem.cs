using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchGroup)), UpdateAfter(typeof(BatchCanvasVertexSystem))]
    [AlwaysSynchronizeSystem]
    public class BuildCanvasMeshSystem : JobComponentSystem {

        private EntityQuery canvasMeshQuery;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            canvasMeshQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<CanvasVertexData>(), ComponentType.ReadOnly<CanvasIndexElement>(),
                    ComponentType.ReadOnly<SubmeshSliceElement>(), ComponentType.ReadOnly<MeshBuildTag>()
                }
            });

            commandBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var cmdBuffer = commandBufferSystem.CreateCommandBuffer();

            Entities.WithStoreEntityQueryInField(ref canvasMeshQuery).WithoutBurst().ForEach((
                Entity entity, 
                Mesh mesh, 
                DynamicBuffer<CanvasVertexData> vertices, 
                DynamicBuffer<CanvasIndexElement> indices, 
                DynamicBuffer<SubmeshSliceElement> submeshDesc) => {

                mesh.Clear();
                mesh.SetVertexBufferParams(vertices.Length, MeshVertexDataExtensions.VertexDescriptors);
                mesh.SetVertexBufferData(vertices.AsNativeArray(), 0, 0, vertices.Length);
                mesh.subMeshCount = submeshDesc.Length;

                Debug.Log(mesh.subMeshCount);

                for (int i = 0; i < submeshDesc.Length; i++) {
                    var current = submeshDesc[i];
                    Debug.Log($"{current.IndexSpan} {current.VertexSpan}");
                    mesh.SetSubMesh(i, new SubMeshDescriptor {
                        baseVertex  = 0,
                        bounds      = default,
                        indexStart  = current.IndexSpan.x,
                        indexCount  = current.IndexSpan.y,
                        firstVertex = current.VertexSpan.x,
                        vertexCount = current.VertexSpan.y,
                        topology    = MeshTopology.Triangles,
                    });
                }
                mesh.UploadMeshData(false);

                cmdBuffer.RemoveComponent<MeshBuildTag>(entity);
            }).Run();

            return inputDeps;
        }
    }
}
