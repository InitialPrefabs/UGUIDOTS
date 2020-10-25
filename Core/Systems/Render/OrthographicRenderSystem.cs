using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
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

        protected override void OnUpdate() {
            cmd.Clear();
            cmd.SetViewProjectionMatrices(
                Matrix4x4.Ortho(0, Screen.width, 0, Screen.height, -100f, 100f), 
                Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));

            // TODO: Enable scissor rect
            // var rect = new Rect(0, 0, Screen.width, Screen.height);
            // cmd.EnableScissorRect(rect);

            Entities.ForEach((SharedMesh mesh, MaterialPropertyBatch batch, DynamicBuffer<SubmeshKeyElement> keys, 
                in ScreenSpace c0) => {

                var submeshKeys = keys.AsNativeArray();
                for (int i = 0; i < mesh.Value.subMeshCount && mesh.Value.subMeshCount == submeshKeys.Length; i++) {
                    var materialKey = submeshKeys[i].MaterialEntity;
                    var prop        = batch.Value[i];
                    var mat         = EntityManager.GetComponentData<SharedMaterial>(materialKey).GetMaterial();

                    var m = Matrix4x4.TRS(default, Quaternion.identity, new float3(1));
                    cmd.DrawMesh(mesh.Value, m, mat, i, -1, prop);
                }
            }).WithoutBurst().Run();
        }
    }
}
