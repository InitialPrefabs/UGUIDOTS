using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    public class CanvasScalerSystem : SystemBase {

        protected override void OnUpdate() {
            var resolution = new float2(Screen.width, Screen.height);

            Entities.WithAll<OnResolutionChangeTag>().ForEach((ref ScreenSpace c0, in ReferenceResolution c1) => {
                var logWidth  = math.log2(resolution.x / c1.Value.x);
                var logHeight = math.log2(resolution.y / c1.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.WidthHeightWeight);
                var scale     = math.pow(2, avg);

                c0.Translation = resolution / 2;
                c0.Scale = scale;
            }).Run();
        }
    }
}
