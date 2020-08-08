using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshRenderGroup))]
    public class OrthographicRenderSystem : SystemBase {

#pragma warning disable 649
        private CommandBuffer cmd;
#pragma warning restore 649

        protected override void OnStartRunning() {
            Entities.ForEach((RenderCommand cmd) => {
                this.cmd = cmd.RenderFeature.InitCommandBuffer();
            }).WithoutBurst().Run();
        }

        protected unsafe override void OnUpdate() {
            cmd?.Clear();
            Entities.ForEach((Mesh mesh, MaterialPropertyBatch batch, DynamicBuffer<SubmeshKeyElement> keys) => {
                if (keys.Length == 0) { 
                    return; 
                }

                for (int i = 0; i < mesh.subMeshCount; i++) {
                    var materialKey = keys[i].MaterialEntity;
                    var prop        = batch.Value[i];
                    var mat         = EntityManager.GetComponentData<SharedMaterial>(materialKey).Value;

                    // Debug.Log($"{cmd == null}, {mesh == null}, {batch.Value == null}, {keys.IsCreated}");

                    cmd.DrawMesh(mesh, Matrix4x4.identity, mat, i, -1, prop);
                }
            }).WithoutBurst().Run();

            /*
            Entities.WithStoreEntityQueryInField(ref renderQuery).
                ForEach((Mesh mesh, MaterialPropertyBatch batch, DynamicBuffer<SubmeshKeyElement> keys) => {
                    renderFeature.Pass.RenderInstructions.Enqueue(new RenderInstruction {
                        Start = (SubmeshKeyElement*)keys.GetUnsafePtr(),
                        Batch = batch,
                        Mesh  = mesh,
                    }
                );
            }).WithoutBurst().Run();
            */
        }
    }
}
