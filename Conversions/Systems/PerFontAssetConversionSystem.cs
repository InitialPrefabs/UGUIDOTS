using TMPro;
using Unity.Mathematics;

namespace UGUIDOTS.Conversions.Systems {
    internal class PerFontAssetConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI text) => {
                var fontAsset = text.font;
                
                // TODO: Maybe a singleton data as a hashmap of unsafe list might just be better...
                if (fontAsset != null) {
                    var fontAssetEntity = GetPrimaryEntity(fontAsset);

#if UNITY_EDITOR
                    DstEntityManager.SetName(fontAssetEntity, $"[Font Asset]: {fontAsset.name}");
#endif

                    DstEntityManager.AddComponentData(fontAssetEntity, new FontID {
                        Value = fontAsset.GetInstanceID()
                    });

                    DstEntityManager.AddComponentData(fontAssetEntity,
                        fontAsset.faceInfo.ToFontFaceInfo(
                            new float2(fontAsset.normalStyle, fontAsset.normalSpacingOffset),
                            new float2(fontAsset.boldStyle, fontAsset.boldSpacing),
                            new int2(fontAsset.atlasWidth, fontAsset.atlasHeight)
                        ));

                    var buffer = DstEntityManager.AddBuffer<GlyphElement>(fontAssetEntity);

                    foreach (var entry in fontAsset.characterLookupTable) {
                        var metrics = entry.Value.glyph.metrics;
                        var rect = entry.Value.glyph.glyphRect;

                        var rawUV = new float4(
                            new float2(rect.x, rect.y),                             // Min
                            new float2(rect.x + rect.width, rect.y + rect.height)   // Max
                        );

                        buffer.Add(new GlyphElement {
                            Unicode  = (ushort)entry.Key,
                            Advance  = metrics.horizontalAdvance,
                            Bearings = new float2(metrics.horizontalBearingX, metrics.horizontalBearingY),
                            Size     = new float2(metrics.width, metrics.height),
                            Scale    = entry.Value.scale,
                            Style    = text.fontStyle,
                            RawUV    = rawUV,
                        });
                    }
                }
            });
        }
    }
}
