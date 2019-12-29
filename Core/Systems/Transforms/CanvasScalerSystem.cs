using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class CanvasScalerSystem : JobComponentSystem {

        private EntityQuery scaleQuery;

        protected override void OnCreate() {
            scaleQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<ReferenceResolution>(),
                    ComponentType.ReadOnly<WidthHeightWeight>(), 
                    ComponentType.ReadWrite<LocalToWorld>()
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var res = new int2(Screen.width, Screen.height);

            return Entities.WithStoreEntityQueryInField(ref scaleQuery)
                .ForEach((ref ReferenceResolution c0, ref WidthHeightWeight c1, ref LocalToWorld c2) => {

                var logWidth  = math.log2(res.x / c0.Value.x);
                var logHeight = math.log2(res.y / c0.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.Value);
                var scale     = math.pow(2, avg);
                var center    = new float3(res / 2, 0);
                c2            = new LocalToWorld { Value = float4x4.TRS(center, default, new float3(scale)) };
            }).Schedule(inputDeps);
        }
    }
}
