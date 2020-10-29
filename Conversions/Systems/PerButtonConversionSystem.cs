using UnityEngine.UI;

namespace UGUIDOTS.Conversions.Systems {

    internal class PerButtonConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Button button) => {
                var entity = GetPrimaryEntity(button);
                var colorBlock = button.colors;
                var colorStates = ColorStates.FromColorBlock(colorBlock);

                DstEntityManager.AddComponentData(entity, colorStates);
                DstEntityManager.AddComponentData(entity, new ButtonState { Value = ButtonVisualState.None });
            });
        }
    }
}
