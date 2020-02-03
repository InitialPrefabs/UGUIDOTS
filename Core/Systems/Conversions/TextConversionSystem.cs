using TMPro;
using UGUIDots.Render;
using UGUIDots.Transforms;
using Unity.Entities;
using Unity.Mathematics;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
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

        /// <summary>
        /// Converts UGUI Text components by adding a buffer to chars to the entity, the dimensions, and 
        /// applied color for shader updates.
        ///
        /// Initially components are marked dirty until the vertices are built.
        /// </summary>
        public class TMPTextConversionSystem : GameObjectConversionSystem {
            protected override void OnUpdate() {
                Entities.ForEach((TextMeshProUGUI c0) => {
                    var entity = GetPrimaryEntity(c0);

                    DstEntityManager.AddComponentData(entity, new BuildTextTag { });
                    DstEntityManager.AddComponentData(entity, new Dimensions   { Value = c0.rectTransform.Int2Size() });
                    DstEntityManager.AddComponentData(entity, new AppliedColor { Value = c0.color });
                    DstEntityManager.AddComponentData(entity, new TextFontID   { Value = c0.font.GetInstanceID() });
                    DstEntityManager.AddComponentData(entity, new TextOptions  {
                        Size = (ushort)c0.fontSize,
                        Style = c0.fontStyle,
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
}
