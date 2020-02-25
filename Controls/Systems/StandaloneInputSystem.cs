using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Controls.Systems {

    [UpdateInGroup(typeof(InputGroup))]
    public class UpdateMousePositionSystem : SystemBase {

        protected override void OnUpdate() {
            var mousePos = new float2(Input.mousePosition.x, Input.mousePosition.y);
            Entities.ForEach((DynamicBuffer<CursorPositionElement> b0) => {
                b0[0] = new CursorPositionElement { Value = mousePos };
            }).WithBurst().Run();
        }
    }

    [UpdateInGroup(typeof(InputGroup))]
    [UpdateAfter(typeof(UpdateMousePositionSystem))]
    public class UpdateMouseStateSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((DynamicBuffer<CursorStateElement> b0,  in PrimaryMouseKeyCode c0) => {
                var clicked = Input.GetKey(c0.Value);
                b0[0] = new CursorStateElement { Value = clicked };
            }).WithBurst().Run();
        }
    }
}
