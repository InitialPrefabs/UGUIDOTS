using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateAfter(typeof(RectTransformConversionSystem))]
    public class CanvasConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {

                var entity       = GetPrimaryEntity(canvas);
                var canvasScaler = canvas.GetComponent<CanvasScaler>();

                switch (canvasScaler.uiScaleMode) {
                    case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                        DstEntityManager.AddComponentData(entity, new ReferenceResolution { 
                            Value = canvasScaler.referenceResolution
                        });

                        // TODO: Should figure out if I want to support shrinking and expanding only...
                        if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight) {
                            DstEntityManager.AddComponentData(entity, new WidthHeightWeight {
                                Value =  canvasScaler.matchWidthOrHeight
                            });
                        } else {
                            Debug.LogError($"{canvasScaler.screenMatchMode} is not supported yet...");
                        }
                        break;
                    default:
                        Debug.LogError($"{canvasScaler.uiScaleMode} is not supported, skipping for now...");
                        break;
                }
            });
        }
    }
}
