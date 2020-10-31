using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDOTS.Controls.Systems {

    public class StandaloneMouseCollisionSystem : SystemBase {

        protected override void OnUpdate() {
            if (HasSingleton<CursorTag>()) {
                return;
            }

            // TODO: Copy the mouse input data to the Cursor data.

            Entities.WithNone<NonInteractableButtonTag>().ForEach((
                ref ClickState c0, 
                ref ButtonState c1, 
                in Dimension c2, 
                in LocalToWorld c3, 
                in ButtonClickType c4) => {

                // TODO: Eventually add checks in for the Button Press Types.
            }).Run();
        }
    }
}
