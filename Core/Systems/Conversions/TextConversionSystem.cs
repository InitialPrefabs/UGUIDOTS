using System;
using UGUIDots.Render;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class FontAssetDeclarationSystem : GameObjectConversionSystem {

        private const string ASCIICharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789`~!@#$%^&*()_+-=[]{}\\|;:'\",<.>/?";

        protected override void OnUpdate() {
            Entities.ForEach((Text text) => {
                var font = text.font;
                if (font != null) {
                    DeclareReferencedAsset(text.font);

                    // Build the ASCII based texts for the time being
                    font.RequestCharactersInTexture(ASCIICharacters);
                }
            });
        }
    }

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class FontAssetConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            if (FontEngine.InitializeFontEngine() != 0) {
                throw new InvalidOperationException("$FontEngine cannot load!");
            }

            Entities.ForEach((Font font) => {
                var entity = GetPrimaryEntity(font);

                var engineError = FontEngine.LoadFontFace(font);

                if (engineError != 0) {
                    throw new InvalidOperationException($"Cannot load {font} due to error code: " + 
                        $"{((FontEngineError)engineError)}! Please make sure the font is Dynamic!");
                }

                var fontFaceInfo = FontEngine.GetFaceInfo().ToFontFaceInfo(font.fontSize);
                DstEntityManager.AddComponentData(entity, fontFaceInfo); 

                var buffer = DstEntityManager.AddBuffer<GlyphElement>(entity);
                SetUpGlyphLib(font.characterInfo, ref buffer);
            });

            FontEngine.DestroyFontEngine();
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

    /// <summary>
    /// Converts UGUI Text components by adding a buffer to chars to the entity, the dimensions, and applied color 
    /// for shader updates.
    ///
    /// Initially components are marked dirty until the vertices are built.
    /// </summary>
    public class UGUITextConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Text c0) => {
                var entity = GetPrimaryEntity(c0);

                // TODO: Remove the disabled tag
                DstEntityManager.AddComponentData(entity, new Dimensions   { Value = c0.rectTransform.Int2Size() });
                DstEntityManager.AddComponentData(entity, new AppliedColor { Value = c0.color });
                DstEntityManager.AddComponentData(entity, new DirtyTag     { });
                DstEntityManager.AddComponentData(entity, new Disabled     { });

                DstEntityManager.AddComponentObject(entity, c0.material);
                AddTextData(entity, c0.text);
            });
        }

        private void AddTextData(Entity e, string text) {
            var length = text.Length;

            var txtBuffer = DstEntityManager.AddBuffer<TextElement>(e);
            txtBuffer.ResizeUninitialized(length);

            for (int i = 0; i < length; i++) {
                txtBuffer[i] = text[i];
            }

            var vertexBuffer = DstEntityManager.AddBuffer<MeshVertexData>(e);
            vertexBuffer.ResizeUninitialized(text.Length);
        }
    }
}
