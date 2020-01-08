using System.Collections.Generic;
using TMPro;
using UGUIDots.Render;
using Unity.Entities;
using UnityEngine.TextCore;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class TMPFontAssetDeclareSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI c0) => {
                DeclareReferencedAsset(c0.font);
            });
        }
    }

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class TMPFontAssetConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach<TMP_FontAsset>((TMP_FontAsset c0) => {
                var entity = GetPrimaryEntity(c0);
                SetUpGlyph(entity, c0.glyphLookupTable);
            });
        }

        private void SetUpGlyph(Entity entity, in Dictionary<uint, Glyph> map) {
            var buffer      = DstEntityManager.AddBuffer<GlyphElement>(entity);
            buffer.Capacity = map.Count;

            foreach (var kv in map) {
                buffer.Add(new GlyphElement {
                    Char      = (ushort)kv.Key,
                    Scale     = kv.Value.scale,
                    GlyphRect = kv.Value.glyphRect,
                    Metrics   = kv.Value.metrics
                });
            }
        }
    }

    public class TextMeshProUGUIConversionSystem : GameObjectConversionSystem {
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

            var buffer = DstEntityManager.AddBuffer<TextElement>(e);
            buffer.ResizeUninitialized(length);

            for (int i = 0; i < length; i++) {
                buffer[i] = text[i];
            }
        }
    }
}
