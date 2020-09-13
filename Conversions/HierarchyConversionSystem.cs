using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UGUIDots.Analyzers;
using Unity.Mathematics;
using UGUIDots.Transforms;
using Unity.Entities;
using UGUIDots.Render;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UGUIDots.Conversions.Systems {

    internal class HierarchyConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {
                if (canvas.transform.parent != null) {
                    Debug.LogError("Cannot convert a canvas that is not a root canvas!");
                    return;
                }

                var batches = BatchAnalysis.BuildStaticBatch(canvas);

                var canvasEntity = GetPrimaryEntity(canvas);
                CanvasConversionUtils.CleanCanvas(canvasEntity, DstEntityManager);
                CanvasConversionUtils.SetScaleMode(canvasEntity, canvas, DstEntityManager);

                ConstructMaterialPropertyBatch(canvasEntity, batches);
                BakeRenderElements(canvasEntity, batches);

                foreach (var element in batches) {
                    BuildPerElement(element);
                }
            });
        }

        private unsafe void BakeRenderElements(Entity canvasEntity, List<List<GameObject>> batches) {
            var renderEntities = new NativeList<RenderElement>(Allocator.Temp);
            var batchSpans     = new NativeList<BatchedSpanElement>(Allocator.Temp);
            int startIdx       = 0;

            // Build a flat array of of the elements we need to render and the spans which defines
            // which sections of the RenderElements belong to which batch.
            // TODO: Write more documents on this...
            for (int i = 0; i < batches.Count; i++) {
                var currentBatch = batches[i];

                for (int k = 0; k < currentBatch.Count; k++) {
                    var uiElement = GetPrimaryEntity(currentBatch[k]);
                    renderEntities.Add(new RenderElement { Value = uiElement });
                }

                batchSpans.Add(new int2(startIdx, currentBatch.Count));
                startIdx += currentBatch.Count;
            }

            var renderBatches = DstEntityManager.AddBuffer<RenderElement>(canvasEntity);
            var size = UnsafeUtility.SizeOf<RenderElement>() * renderEntities.Length;

            renderBatches.ResizeUninitialized(renderEntities.Length);
            UnsafeUtility.MemCpy(renderBatches.GetUnsafePtr(), renderEntities.GetUnsafePtr(), size);

            var renderSpans = DstEntityManager.AddBuffer<BatchedSpanElement>(canvasEntity);
            size = UnsafeUtility.SizeOf<BatchedSpanElement>() * batchSpans.Length;
            
            renderSpans.ResizeUninitialized(batchSpans.Length);
            UnsafeUtility.MemCpy(renderSpans.GetUnsafePtr(), batchSpans.GetUnsafePtr(), size);
        }

        private void ConstructMaterialPropertyBatch(Entity canvasEntity, List<List<GameObject>> batches) {
            var propertyBatch = new MaterialPropertyBatch {
                Value = new MaterialPropertyBlock[batches.Count]
            };

            for (int i = 0; i < batches.Count; i++) {
                var block = new MaterialPropertyBlock();

                if (batches[i][0].TryGetComponent(out Image image)) {
                    var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                    block.SetTexture(ShaderIDConstants.MainTex, texture);

                    for (int k = 0; k < batches[i].Count; k++) {
                        var entity = GetPrimaryEntity(batches[i][k]);
                        DstEntityManager.AddComponentData(entity, new MaterialPropertyIndex { Value = (ushort)i });
                    }
                }

                propertyBatch.Value[i] = block;
            }

            DstEntityManager.AddComponentData(canvasEntity, propertyBatch);
        }

        private void BuildPerElement(List<GameObject> batch) {
            foreach (var gameObject in batch) {
                if (gameObject.TryGetComponent(out Image image)) {
                    var imgEntity = GetPrimaryEntity(image);

                    DstEntityManager.AddComponentData(imgEntity, new AppliedColor { Value = image.color });
                    ImageConversionUtils.SetImageType(imgEntity, image, DstEntityManager);

                    // Set up the texture
                    var rectSize = image.rectTransform.Int2Size();
                    var spriteResolution = image.sprite != null ? 
                        new int2(image.sprite.texture.width, image.sprite.texture.height) :
                        rectSize;

                    DstEntityManager.AddComponentData(imgEntity, new DefaultSpriteResolution { 
                        Value = spriteResolution 
                    });

                    // Set up the sprite
                    DstEntityManager.AddComponentData(imgEntity, SpriteData.FromSprite(image.sprite));
                }

                if (gameObject.TryGetComponent(out TMP_Text txt)) {
                    var txtEntity = GetPrimaryEntity(txt);
                    DstEntityManager.AddComponentData(txtEntity, new AppliedColor { Value = txt.color });
                    DstEntityManager.AddComponentData(txtEntity, new TextOptions {
                        Size      = (ushort)txt.fontSize,
                        Style     = txt.fontStyle,
                        Alignment = txt.alignment.FromTextAnchor()
                    });
                }
            }
        }
    }
}
