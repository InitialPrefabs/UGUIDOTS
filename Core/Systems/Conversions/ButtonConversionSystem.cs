using Unity.Entities;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    [UpdateAfter(typeof(ImageConversionSystem))]
    public class ButtonConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Button button) => {
                var entity = GetPrimaryEntity(button);

                var colorBlock = button.colors;
                DstEntityManager.AddComponentData(entity, ClickState.Default());
                DstEntityManager.AddComponentData(entity, ButtonVisual.Default());
                DstEntityManager.AddComponentData(entity, ColorStates.FromColorBlock(colorBlock));

                if (!button.interactable) {
                    DstEntityManager.AddComponentData(entity, new ButtonDisabledTag { });
                    DstEntityManager.SetComponentData(entity, new AppliedColor { Value = colorBlock.disabledColor });
                    DstEntityManager.RemoveComponent<Disabled>(entity);
                } else {
                    DstEntityManager.SetComponentData(entity, new AppliedColor { Value = colorBlock.normalColor });
                }
            });
        }
    }
}
