using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class CursorCollisionSystem : SystemBase {

        protected override void OnUpdate() {
            if (!HasSingleton<CursorTag>()) {
#if UNITY_EDITOR
                Debug.LogError("No Entity with the type: CursorBuffer found!");
#endif
                return;
            }

            var cursorEntity = GetSingletonEntity<CursorTag>();
            var cursors = GetBuffer<Cursor>(cursorEntity).AsNativeArray();

            Entities.ForEach((Entity entity, ref ButtonState c0, in Dimension c1, in ScreenSpace c2) => {
                var aabb    = new AABB {
                    Center  = new float3(c2.Translation, 0),
                    Extents = new float3(c1.Extents(), 0)
                };

                for (int i = 0; i < cursors.Length; i++) {
                    var cursor = cursors[i].Value;
                    var position = new float3(cursor.Position, 0);

                    if (aabb.Contains(position)) {
                        c0.Value = cursor.Pressed ? ButtonVisualState.Hover : ButtonVisualState.Pressed;
                    }
                }
            }).WithReadOnly(cursors).Run();

        }
    }
}
