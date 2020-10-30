using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class CursorCollisionSystem : SystemBase {

        protected override void OnUpdate() {
            if (!HasSingleton<CursorBuffer>()) {
#if UNITY_EDITOR
                Debug.LogError("No Entity with the type: CursorBuffer found!");
#endif
                return;
            }

            var cursors = GetSingleton<CursorBuffer>();

            // TODO: Update the button visuals
            Entities.ForEach((Entity entity, 
                ref ButtonState c0,
                in RootCanvasReference c1, 
                in ColorStates c3, 
                in Dimension c4,
                in ScreenSpace c5) => {

                var aabb = new AABB {
                    Center = new float3(c5.Translation, 0),
                    Extents = new float3(c4.Extents(), 0)
                };

                for (int i = 0; i < cursors.Length; i++) {
                    var cursor = cursors[i];
                    var position = new float3(cursor.Position, 0);

                    if (aabb.Contains(position)) {
                        c0.Value = ButtonVisualState.Hover;
                    }
                }
            }).Run();

        }
    }
}
