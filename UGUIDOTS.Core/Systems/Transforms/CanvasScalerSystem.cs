using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public class CanvasScalerSystem : SystemBase {

        struct RecurseStretchedChildren {

            public int2 Resolution;

            [ReadOnly]
            public ComponentDataFromEntity<Stretch> Stretch;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            public ComponentDataFromEntity<Dimension> Dimensions;

            public ComponentDataFromEntity<ScreenSpace> ScreenSpace;

            public void Execute(Entity entity) {
                if (Children.HasComponent(entity)) {
                    var children = Children[entity].AsNativeArray().AsReadOnly();
                    Recurse(children);
                }
            }

            void Recurse(NativeArray<Child>.ReadOnly children) {
                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;   

                    if (Stretch.HasComponent(current)) {
                        var screenSpace = ScreenSpace[current];
                        screenSpace.Translation = Resolution / 2;
                        ScreenSpace[current] = screenSpace;
                        Dimensions[current] = new Dimension { Value = Resolution };
                    }

                    if (Children.HasComponent(current)) {
                        var grandChildren = Children[current].AsNativeArray().AsReadOnly();
                        Recurse(grandChildren);
                    }
                }
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var resolution  = new int2(Screen.width, Screen.height);
            var cmdBuffer   = cmdBufferSystem.CreateCommandBuffer();
            var stretched   = GetComponentDataFromEntity<Stretch>(true);

            // NOTE: Rebuilding all stretched image positions and dimensions, since they're pretty much like canvas.
            var recurse     = new RecurseStretchedChildren {
                Dimensions  = GetComponentDataFromEntity<Dimension>(false),
                ScreenSpace = GetComponentDataFromEntity<ScreenSpace>(false),
                Stretch     = GetComponentDataFromEntity<Stretch>(true),
                Children    = GetBufferFromEntity<Child>(true),
                Resolution  = resolution,
            };

            Entities.WithNone<OnResolutionChangeTag>().ForEach(
                (Entity entity, in ScreenSpace c0, in Dimension c1, in ReferenceResolution c2) => {
                if (!c1.Value.Equals(resolution)) {
                    var logWidth  = math.log2((float)resolution.x / c2.Value.x);
                    var logHeight = math.log2((float)resolution.y / c2.Value.y);
                    var avg       = math.lerp(logWidth, logHeight, c2.WidthHeightWeight);
                    var scale     = math.pow(2, avg);

                    var screenSpace = new ScreenSpace {
                        Translation = resolution / 2,
                        Scale = scale
                    };

                    recurse.Execute(entity);

                    cmdBuffer.SetComponent(entity, screenSpace);
                    cmdBuffer.SetComponent(entity, new Dimension { Value = resolution });
                    cmdBuffer.AddComponent<OnResolutionChangeTag>(entity);
                }
            }).Run();
        }
    }
}
