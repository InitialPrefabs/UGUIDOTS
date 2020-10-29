using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS.Transforms.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class CursorCollisionSystem : SystemBase {

        protected override void OnCreate() {
            RequireSingletonForUpdate<Cursor>();
        }

        protected override void OnUpdate() {
            var cursorEntity = GetSingletonEntity<Cursor>();
            var cursors      = EntityManager.GetBuffer<Cursor>(cursorEntity).AsNativeArray();

            Entities.ForEach((Entity entity, in ScreenSpace c0, in Dimension c1) => {
                var center = c1.Center() + c0.Translation;

                var aabb    = new AABB {
                    Center  = new float3(center, 0),
                    Extents = new float3(c1.Extents(), 0)
                };

                for (int i = 0; i < cursors.Length; i++) {
                    float3 current = cursors[i];

                    if (aabb.Contains(current)) {
                    }
                }

            }).WithReadOnly(cursors).Run();
        }
    }
}
