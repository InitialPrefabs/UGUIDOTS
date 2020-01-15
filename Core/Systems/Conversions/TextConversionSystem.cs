using UGUIDots.Render;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class FontAssetDeclarationSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Text text) => {
                if (text.font != null) {
                    DeclareReferencedAsset(text.font);
                }
            });
        }
    }

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class FontAssetConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Font font) => {
                var entity = GetPrimaryEntity(font);

                DstEntityManager.AddComponentData(entity, new FontInfo {
                    DefaultFontSize = font.fontSize,
                    LineHeight      = font.lineHeight,
                    BaseLine        = 0
                });

                var buffer = DstEntityManager.AddBuffer<GlyphElement>(entity);
                SetUpGlyphLib(font.characterInfo, ref buffer);
            });
        }

        private void SetUpGlyphLib(in CharacterInfo[] info, ref DynamicBuffer<GlyphElement> buffer) {
            for (int i = 0; i < info.Length; i++) {
                var characterInfo = info[i];

                buffer.Add(new GlyphElement {
                    Char     = (ushort)characterInfo.index,
                    Advance  = characterInfo.Advance(),
                    Bearings = new float2(characterInfo.BearingX(), characterInfo.BearingY(0)),
                    Size     = new float2(characterInfo.Width(), characterInfo.Height()),
                    UV       = new float2x4(characterInfo.uvBottomLeft, characterInfo.uvTopLeft,
                            characterInfo.uvTopRight, characterInfo.uvBottomRight)
                });
            }
        }
    }

    public class UGUITextConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Text c0) => {
                var entity = GetPrimaryEntity(c0);

                DstEntityManager.AddComponentData(entity, new Dimensions           { Value = c0.rectTransform.Int2Size() });
                DstEntityManager.AddComponentData(entity, new AppliedColor         { Value = c0.color });
                DstEntityManager.AddSharedComponentData(entity, new RenderMaterial { Value = c0.material });
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
