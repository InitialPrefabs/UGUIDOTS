using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CanvasScalerSystem : JobComponentSystem {

        private struct ResizeCanvasJob : IJobForEach<ReferenceResolution, WidthHeightWeight, LocalToWorld> {

            public int2 Resolution;

            public void Execute([ReadOnly] ref ReferenceResolution c0, [ReadOnly] ref WidthHeightWeight c1, ref LocalToWorld c2) {
                var logWidth  = math.log2(Resolution.x / c0.Value.x);
                var logHeight = math.log2(Resolution.y / c0.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.Value);
                var scale     = math.pow(2, avg);
                var center    = new float3(Resolution / 2, 0);
                c2            = new LocalToWorld { Value = float4x4.TRS(center, c2.Rotation, new float3(scale)) };
            }
        }

        private EntityQuery scaleQuery;
        private int2 res;

        protected override void OnCreate() {
            scaleQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<ReferenceResolution>(),
                    ComponentType.ReadOnly<WidthHeightWeight>(), 
                    ComponentType.ReadWrite<LocalToWorld>()
                }
            });

            res = new int2(Screen.width, Screen.height);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var current = new int2(Screen.width, Screen.height);
            return new ResizeCanvasJob {
                Resolution = current
            }.Schedule(this, inputDeps);
        }
    }
}
