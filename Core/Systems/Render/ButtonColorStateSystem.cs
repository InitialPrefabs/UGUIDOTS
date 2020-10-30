using UGUIDOTS.Transforms.Systems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(CursorCollisionSystem))]
    public unsafe class ButtonColorStateSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter();

            var vertexBuffers = GetBufferFromEntity<Vertex>(false);

            Entities.WithNone<NonInteractableButtonTag>().ForEach((
                Entity entity, 
                ref AppliedColor c0, 
                in ColorStates c1, 
                in ButtonState c2,
                in RootCanvasReference c3,
                in MeshDataSpan c4) => {

                EquatableColor32 color = c0.Value;
                bool delta = true;
                
                switch (c2.Value) {
                    case var _ when ButtonVisualState.Hover == c2.Value && !color.Equals(c1.HighlightedColor):
                        c0.Value = c1.HighlightedColor;
                        break;
                    case var _ when ButtonVisualState.Pressed == c2.Value && !color.Equals(c1.PressedColor):
                        c0.Value = c1.PressedColor;
                        break;
                    case var _ when ButtonVisualState.None == c2.Value && !color.Equals(c1.DefaultColor):
                        c0.Value = c1.DefaultColor;
                        break;
                    default:
                        delta = false;
                        break;
                } 

                if (delta) {
                    var vertices = vertexBuffers[c3.Value];
                    var vertexSpan = c4.VertexSpan;
                    for (int i = 0; i < vertexSpan.y; i++) {
                        var index = i + vertexSpan.x;
                        var vertex = vertices[index];
                    }
                }
            }).Run();

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
