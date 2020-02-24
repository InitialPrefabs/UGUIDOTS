using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    [UpdateInGroup(typeof(InputGroup))]
    public class UpdateMousePositionSystem : JobComponentSystem {

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var mousePos = new float2(Input.mousePosition.x, Input.mousePosition.y);
            return Entities.ForEach((DynamicBuffer<CursorPositionElement> b0) => {
                b0[0] = new CursorPositionElement { Value = mousePos };
            }).WithBurst().Schedule(inputDeps);
        }
    }

    [UpdateInGroup(typeof(InputGroup))]
    public class UpdateMouseStateSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Entities.ForEach((DynamicBuffer<CursorStateElement> b0,  in PrimaryMouseKeyCode c0) => {
                var clicked = Input.GetKey(c0.Value);
                b0[0] = new CursorStateElement { Value = clicked };
            }).Run();
            return inputDeps;
        }
    }
}
