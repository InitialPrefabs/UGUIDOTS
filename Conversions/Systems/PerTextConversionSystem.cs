using TMPro;
using UGUIDOTS.Render;
using UGUIDOTS.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Conversions.Systems {
    internal class PerTextConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((TextMeshProUGUI text) => {
                var material = text.materialForRendering;

                if (material == null) {
                    Debug.LogError("A material is missing from the TextMeshUGUI component, conversion will not work!");
                    return;
                }

                var materialEntity = GetPrimaryEntity(material);
                DstEntityManager.AddComponentData(materialEntity, new SharedMaterial { Value = material });

#if UNITY_EDITOR
                DstEntityManager.SetName(materialEntity, $"[Material]: {material.name}");
#endif

                var textEntity = GetPrimaryEntity(text);
                DstEntityManager.AddComponentData(textEntity, new LinkedMaterialEntity { Value = materialEntity });
                DstEntityManager.AddComponentData(textEntity, new AppliedColor { Value = text.color });
                DstEntityManager.AddComponentData(textEntity, new TextOptions {
                    Size      = (ushort)text.fontSize,
                    Style     = text.fontStyle,
                    Alignment = text.alignment.FromTextAnchor()
                });

                AddTextData(textEntity, text.text);
            });
        }

        private unsafe void AddTextData(Entity e, string text) {
            var length = text.Length;

            var txtBuffer = DstEntityManager.AddBuffer<CharElement>(e);
            txtBuffer.ResizeUninitialized(length);

            fixed (char* start = text) {
                UnsafeUtility.MemCpy(txtBuffer.GetUnsafePtr(), start, UnsafeUtility.SizeOf<char>() * length);
            }
        }
    }
}
