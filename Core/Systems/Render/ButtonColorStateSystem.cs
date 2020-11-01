using System.Runtime.CompilerServices;
using UGUIDOTS.Transforms.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(CursorCollisionSystem))]
    public unsafe class ButtonColorStateSystem : SystemBase {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Equals(Color32 lhs, Color32 rhs) {
            return lhs.a == rhs.a && lhs.r == rhs.r && lhs.b == rhs.b && lhs.g == rhs.g;
        }

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery canvasQuery;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<ReferenceResolution>() }
            });
        }

        protected override void OnUpdate() {
            var vertexBuffers = GetBufferFromEntity<Vertex>(false);
            var hashSet       = new NativeHashSet<Entity>(canvasQuery.CalculateEntityCount(), Allocator.Temp);

            Entities.WithNone<NonInteractableButtonTag>().ForEach((
                ref AppliedColor c0, 
                in ColorStates c1, 
                in ButtonState c2,
                in RootCanvasReference c3,
                in MeshDataSpan c4) => {

                Color32 color = c0.Value;
                bool delta = true;
                
                switch (c2.Value) {
                    case var _ when ButtonVisualState.Hover == c2.Value && !Equals(color, c1.HighlightedColor):
                        c0.Value = c1.HighlightedColor;
                        break;
                    case var _ when ButtonVisualState.Pressed == c2.Value && !Equals(color, c1.PressedColor):
                        c0.Value = c1.PressedColor;
                        break;
                    case var _ when ButtonVisualState.None == c2.Value && !Equals(color, c1.DefaultColor):
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
                        var index       = i + vertexSpan.x;
                        var vertex      = vertices[index];
                        vertex.Color    = c0.Value.ToNormalizedFloat4();
                        vertices[index] = vertex;
                    }
                    hashSet.Add(c3.Value);
                }
            }).Run();

            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            Job.WithCode(() => {
                var entities = hashSet.ToNativeArray(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++) {
                    cmdBuffer.AddComponent<RebuildMeshTag>(entities[i]);
                }
            }).Run();

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
