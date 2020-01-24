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

        // TODO: Move to unicode instead - this is only temporary
        private const string ASCIICharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789`~!@#$%^&*()_+-=[]{}\\|;:'\",<.>/? \n";

        protected override void OnUpdate() {
            Entities.ForEach((Text text) => {
                var font = text.font;
                if (font != null) {
                    DeclareReferencedAsset(text.font);

                    // TODO: Support other languages
                    // Build the ASCII based texts for the time being
                    font.RequestCharactersInTexture(ASCIICharacters, font.fontSize, FontStyle.Normal);
                    font.RequestCharactersInTexture(ASCIICharacters, font.fontSize, FontStyle.Italic);
                    font.RequestCharactersInTexture(ASCIICharacters, font.fontSize, FontStyle.Bold);
                    font.RequestCharactersInTexture(ASCIICharacters, font.fontSize, FontStyle.BoldAndItalic);
                }
            });
        }
    }

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class FontAssetConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            if (FontEngine.InitializeFontEngine() != 0) {
                throw new InvalidOperationException("FontEngine cannot load!");
            }

            Entities.ForEach((Font font) => {
                var entity = GetPrimaryEntity(font);

                var engineError = FontEngine.LoadFontFace(font);

                if (engineError != 0) {
                    throw new InvalidOperationException($"Cannot load {font}, because the font is not dynamic!");
                }

                var fontFaceInfo = FontEngine.GetFaceInfo().ToFontFaceInfo(font.fontSize);
                DstEntityManager.AddComponentData(entity, fontFaceInfo);

                var buffer = DstEntityManager.AddBuffer<GlyphElement>(entity);
                SetUpGlyphLib(font.characterInfo, ref buffer);

                DstEntityManager.AddComponentData(entity, new FontID { Value = font.GetInstanceID() });
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
                    Style    = characterInfo.style,
                    UV       = new float2x4(characterInfo.uvBottomLeft, characterInfo.uvTopLeft,
                            characterInfo.uvTopRight, characterInfo.uvBottomRight)
                });
            }
        }
    }

    /// <summary>
    /// Converts UGUI Text components by adding a buffer to chars to the entity, the dimensions, and 
    /// applied color for shader updates.
    ///
    /// Initially components are marked dirty until the vertices are built.
    /// </summary>
    public class UGUITextConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Text c0) => {
                var entity = GetPrimaryEntity(c0);

                DstEntityManager.AddComponentData(entity, new BuildTextTag { });
                DstEntityManager.AddComponentData(entity, new Dimensions   { Value = c0.rectTransform.Int2Size() });
                DstEntityManager.AddComponentData(entity, new AppliedColor { Value = c0.color });
                DstEntityManager.AddComponentData(entity, new TextFontID   { Value = c0.font.GetInstanceID() });
                DstEntityManager.AddComponentData(entity, new TextOptions  {
                    Size      = (ushort)c0.fontSize,
                    Style     = c0.fontStyle,
                    Alignment = c0.alignment
                });

                DstEntityManager.AddComponentObject(entity, c0.font.material);
                AddTextData(entity, c0.text);
            });
        }

        private void AddTextData(Entity e, string text) {
            var length = text.Length;

            var txtBuffer = DstEntityManager.AddBuffer<CharElement>(e);
            txtBuffer.ResizeUninitialized(length);

            for (int i = 0; i < length; i++) {
                txtBuffer[i] = text[i];
            }

            var vertexBuffer = DstEntityManager.AddBuffer<MeshVertexData>(e);
            vertexBuffer.ResizeUninitialized(text.Length);

            for (int i = 0; i < text.Length; i++) {
                vertexBuffer[i] = default;
            }

            var indexBuffer = DstEntityManager.AddBuffer<TriangleIndexElement>(e);
        }
    }
}
