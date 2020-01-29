using System;
using UGUIDots.Render;
using UGUIDots.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    public static class ASCIIConstants 
    {
        public const string ASCIICharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789`~!@#$%^&*()_+-=[]{}\\|;:'\",<.>/? \n";
    }

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class FontAssetDeclarationSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Text text) => {
                var font = text.font;
                if (font != null) {
                    DeclareReferencedAsset(text.font);

                    // TODO: Support other languages
                    // Build the ASCII based texts for the time being
                    font.RequestCharactersInTexture(ASCIIConstants.ASCIICharacters, font.fontSize, FontStyle.Normal);
                    font.RequestCharactersInTexture(ASCIIConstants.ASCIICharacters, font.fontSize, FontStyle.Italic);
                    font.RequestCharactersInTexture(ASCIIConstants.ASCIICharacters, font.fontSize, FontStyle.Bold);
                    font.RequestCharactersInTexture(ASCIIConstants.ASCIICharacters, font.fontSize, FontStyle.BoldAndItalic);
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

                var engineError = FontEngine.LoadFontFace(font, font.fontSize);

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
                    Unicode  = (ushort)characterInfo.index,
                    Advance  = characterInfo.Advance(),
                    Bearings = new float2(characterInfo.BearingX(), characterInfo.BearingY(0)),
                    Size     = new float2(characterInfo.Width(), characterInfo.Height()),
                    Style    = characterInfo.style,
                    UV       = new float2x4(characterInfo.uvBottomLeft, characterInfo.uvTopLeft,
                            characterInfo.uvTopRight, characterInfo.uvBottomRight),
#if UNITY_EDITOR    
                    Char     = (char)characterInfo.index
#endif
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
                    Alignment = c0.alignment.FromTextAnchor()
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
