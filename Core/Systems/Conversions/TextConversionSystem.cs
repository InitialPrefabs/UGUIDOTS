﻿using TMPro;
using UGUIDOTS.Render;
using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDOTS.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    [DisableAutoCreation]
    public class FontAssetDeclarationSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI txt) => {
                if (txt.font != null) {
                    DeclareReferencedAsset(txt.font);
                }
            });
        }
    }

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    [DisableAutoCreation]
    public class FontAssetConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {

            Entities.ForEach((TextMeshProUGUI text) => {
                var fontAsset = text.font;

                if (fontAsset != null) {
                    var fontAssetEntity = GetPrimaryEntity(fontAsset);

                    DstEntityManager.AddComponentData(fontAssetEntity, new FontID {
                        Value = fontAsset.GetInstanceID()
                    });

                    DstEntityManager.AddComponentData(fontAssetEntity,
                        fontAsset.faceInfo.ToFontFaceInfo(
                            new float2(fontAsset.normalStyle, fontAsset.normalSpacingOffset),
                            new float2(fontAsset.boldStyle, fontAsset.boldSpacing),
                            new int2(fontAsset.atlasWidth, fontAsset.atlasHeight)
                        ));

                    var buffer      = DstEntityManager.AddBuffer<GlyphElement>(fontAssetEntity);
                    buffer.Capacity = fontAsset.characterLookupTable.Count;

                    buffer.ResizeUninitialized(buffer.Capacity);

                    var i = 0;
                    foreach (var entry in fontAsset.characterLookupTable) {
                        var metrics = entry.Value.glyph.metrics;
                        var rect = entry.Value.glyph.glyphRect;

                        var rawUV = new float4(
                            new float2(rect.x, rect.y), // Min
                            new float2(rect.x + rect.width, rect.y + rect.height) // Max
                        );

                        buffer[i]    = new GlyphElement {
                            Unicode  = (ushort)entry.Key,
                            Advance  = metrics.horizontalAdvance,
                            Bearings = new float2(metrics.horizontalBearingX, metrics.horizontalBearingY),
                            Size     = new float2(metrics.width, metrics.height),
                            Scale    = entry.Value.scale,
                            Style    = text.fontStyle,
                            RawUV    = rawUV,
#if UNITY_EDITOR
                            Char     = (char)entry.Key
#endif
                        };
                        i++;
                    }
                }
            });
        }
    }

    /// <summary>
    /// Converts UGUI Text components by adding a buffer to chars to the entity, the dimensions, and
    /// applied color for shader updates.
    /// </summary>
    public class TMPTextConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI text) => {
                var entity = GetPrimaryEntity(text);

                DstEntityManager.AddComponentData(entity, new AppliedColor { Value = text.color });
                DstEntityManager.AddComponentData(entity, new TextFontID   { Value = text.font.GetInstanceID() });
                DstEntityManager.AddComponentData(entity, new TextOptions  {
                    Size      = (ushort)text.fontSize,
                    Style     = text.fontStyle,
                    Alignment = text.alignment.FromTextAnchor()
                });

                DstEntityManager.AddComponentData(entity, new LinkedMaterialEntity { 
                    Value = GetPrimaryEntity(text.materialForRendering) 
                });

                // Marks that the text element needs to be built
                DstEntityManager.AddComponent<BuildUIElementTag>(entity);

                AddTextData(entity, text.text);

                if (!text.gameObject.activeInHierarchy || !text.enabled) {
                    DstEntityManager.AddComponent<UpdateVertexColorTag>(entity);
                }
            });
        }

        private void AddTextData(Entity e, string text) {
            var length = text.Length;

            var txtBuffer = DstEntityManager.AddBuffer<CharElement>(e);
            txtBuffer.ResizeUninitialized(length);

            for (int i = 0; i < length; i++) {
                txtBuffer[i] = text[i];
            }

            var vertexBuffer = DstEntityManager.AddBuffer<LocalVertexData>(e);
            vertexBuffer.ResizeUninitialized(text.Length);

            for (int i = 0; i < text.Length; i++) {
                vertexBuffer[i] = default;
            }

            var indexBuffer = DstEntityManager.AddBuffer<LocalTriangleIndexElement>(e);
        }
    }
}
