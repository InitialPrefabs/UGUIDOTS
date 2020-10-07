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
            var cmdBuffer     = cmdBufferSystem.CreateCommandBuffer();
            var vertexBuffers = GetBufferFromEntity<RootVertexData>(false);
            var screenSpace   = GetComponentDataFromEntity<ScreenSpace>(true);

            Entities.WithAll<UpdateSliceTag, ScreenSpace>().ForEach(
                (Entity entity, in MeshDataSpan c0, in RootCanvasReference c1, in Dimension c2, 
                 in SpriteData c3, in DefaultSpriteResolution c4) => {

                var buffer  = vertexBuffers[c1.Value];
                var current = screenSpace[entity];
                var root    = screenSpace[c1.Value];

                var adjustedWidth = c2.Value.x * root.Scale.x;
                var scale         = (float)c2.Value.x / adjustedWidth;

                var width = c2.Value.x * scale;
                var height = c2.Value.y * scale;
                var dim = new Dimension {
                    Value = new int2((int)width, (int)height)
                };

                // TODO: Check if I have to do a recursive strategy - collect all the parent scales
                var position = ImageUtils.BuildImageVertexData(c4, c3, dim, current.AsMatrix(), scale);

                ImageUtils.UpdateVertexDimension(
                    (RootVertexData*)buffer.GetUnsafePtr(), 
                    c0.VertexSpan, 
                    position);

                // Remove the update slice tag and add the RebuildMeshTag to the root canvas
                cmdBuffer.RemoveComponent<UpdateSliceTag>(entity);
                cmdBuffer.AddComponent<RebuildMeshTag>(c1.Value);
            }).Run();
        }
    }
}
