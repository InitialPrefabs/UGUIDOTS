using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Render.Systems {

    public class UpdateMeshSliceSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected unsafe override void OnUpdate() {
            var resolution = new int2(Screen.width, Screen.height);
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();
            var vertexBuffers = GetBufferFromEntity<RootVertexData>(false);

            Entities.WithAll<UpdateSliceTag>().ForEach(
                (Entity entity, in MeshDataSpan c0, in RootCanvasReference c1, in Dimension c2, 
                 in SpriteData c3, in DefaultSpriteResolution c4, in ScreenSpace c5) => {

                var buffer = vertexBuffers[c1.Value];
                var position = ImageUtils.BuildImageVertexData(c4, c3, c2, c5.AsMatrix());
                ImageUtils.UpdateVertexDimension((RootVertexData*)buffer.GetUnsafePtr(), c0.VertexSpan, position);
                cmdBuffer.RemoveComponent<UpdateSliceTag>(entity);

                // TODO: The canvas mesh needs to be rebuilt.
            }).Run();
        }
    }
}
