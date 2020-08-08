using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshRenderGroup))]
    public class OrthographicRenderSystem : SystemBase {

        private CommandBuffer cmd;

        protected override void OnDestroy() {
            CommandBufferPool.Release(cmd);
        }

        protected override void OnStartRunning() {
            Entities.ForEach((RenderCommand c0) => {
                cmd = CommandBufferPool.Get(c0.RenderFeature.name);
                c0.RenderFeature.CommandBuffer = cmd;
            }).WithoutBurst().Run();
        }

        protected unsafe override void OnUpdate() {
            cmd.Clear();
            cmd.SetViewProjectionMatrices(
                Matrix4x4.Ortho(0, Screen.width, 0, Screen.height, -100f, 100f), 
                Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
            
            Entities.ForEach((Mesh mesh, MaterialPropertyBatch batch, DynamicBuffer<SubmeshKeyElement> keys) => {
                var submeshKeys = keys.AsNativeArray();
                for (int i = 0; i < mesh.subMeshCount && mesh.subMeshCount == submeshKeys.Length; i++) {
                    var materialKey = submeshKeys[i].MaterialEntity;
                    var prop        = batch.Value[i];
                    var mat         = EntityManager.GetComponentData<SharedMaterial>(materialKey).Value;

                    cmd.DrawMesh(mesh, Matrix4x4.identity, mat, i, -1, prop);
                }
            }).WithoutBurst().Run();
        }
    }
}
