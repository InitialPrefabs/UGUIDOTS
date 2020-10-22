using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class ResolutionDeltaSystem : SystemBase {

        private float2 resolution;
        private EntityQuery rebuildQuery;
        private EntityQuery builtQuery;

        protected override void OnCreate() {
            resolution = new float2(Screen.width, Screen.height);

            rebuildQuery = GetEntityQuery(new EntityQueryDesc {
                All     = new [] { ComponentType.ReadOnly<ReferenceResolution>() },
                None    = new [] { ComponentType.ReadOnly<OnResolutionChangeTag>() },
                Options = EntityQueryOptions.IncludeDisabled
            });

            builtQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<ReferenceResolution>(), 
                    ComponentType.ReadOnly<OnResolutionChangeTag>() 
                },
                Options = EntityQueryOptions.IncludeDisabled
            });
        }

        protected override void OnUpdate() {
            if (builtQuery.CalculateEntityCount() > 0) {
                EntityManager.RemoveComponent<OnResolutionChangeTag>(builtQuery);
            }

            var screen = new float2(Screen.width, Screen.height);

            if (!screen.Equals(resolution)) {
                EntityManager.AddComponent<OnResolutionChangeTag>(rebuildQuery);
                resolution = screen;
            }
        }
    }
}
