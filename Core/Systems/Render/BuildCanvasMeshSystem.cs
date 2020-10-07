using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class BuildCanvasMeshSystem : SystemBase {

        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            commandBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = commandBufferSystem.CreateCommandBuffer();

            Entities.WithAll<RebuildMeshTag>().ForEach((
                Entity entity,
                SharedMesh s0,
                DynamicBuffer<RootVertexData> b0,
                DynamicBuffer<RootTriangleIndexElement> b1,
                DynamicBuffer<SubmeshSliceElement> b2) => {

                var mesh = s0.Value;

                mesh.Clear();
                mesh.SetVertexBufferParams(b0.Length, MeshVertexDataExtensions.VertexDescriptors);
                mesh.SetVertexBufferData(b0.AsNativeArray(), 0, 0, b0.Length);
                mesh.SetIndexBufferParams(b1.Length, IndexFormat.UInt16);
                mesh.SetIndexBufferData(b1.AsNativeArray(), 0, 0, b1.Length);

                mesh.subMeshCount = b2.Length;
                var submeshes = b2.AsNativeArray();

                for (int i = 0; i < b2.Length; i++) {
                    var current = submeshes[i];

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
            }).WithoutBurst().Run();
        }
    }
}
