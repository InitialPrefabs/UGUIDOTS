using TMPro;
using UGUIDots.Render;
using Unity.Entities;

namespace UGUIDots.Conversions.Systems {

    public class TextConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI c0) => {
                var entity = GetPrimaryEntity(c0);

                DstEntityManager.AddComponentData(entity, new Dimensions           { Value = c0.rectTransform.Int2Size() });
                DstEntityManager.AddComponentData(entity, new AppliedColor         { Value = c0.color });
                DstEntityManager.AddSharedComponentData(entity, new RenderMaterial { Value = c0.fontSharedMaterial });
                AddTextComponent(entity, c0.text);
            });
        }

        private void AddTextComponent(Entity e, string text) {
            var length = text.Length;

            if (text.Length <= 30) {
                DstEntityManager.AddComponentData(e, new Text32 { Value = text });
            } else if (text.Length <= 62) {
                DstEntityManager.AddComponentData(e, new Text64 { Value = text });
            } else if (text.Length <= 126) {
                DstEntityManager.AddComponentData(e, new Text128 { Value = text });
            } else if (text.Length <= 510) {
                DstEntityManager.AddComponentData(e, new Text512 { Value = text });
            } else if (text.Length <= 4096) {
                DstEntityManager.AddComponentData(e, new Text4096 { Value = text });
            } else {
                throw new System.ArgumentOutOfRangeException("Text too long.");
            }
        }
    }
}
