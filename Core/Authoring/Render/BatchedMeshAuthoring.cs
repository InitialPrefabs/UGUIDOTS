using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Render.Authoring {

    public class BatchedMeshAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        // Used for serialization cases of jagged arrays
        [System.Serializable]
        public struct BatchedElements {
            public GameObject[] Elements;
        }

        public BatchedElements[] Batches;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var renderEntities = new NativeList<RenderElement>(Allocator.Temp);
            var batchSpans     = new NativeList<BatchedSpanElement>(Allocator.Temp);

            // Flat map the batch of render elements to a single array.
            int startIndex = 0;
            for (int i = 0; i < Batches.Length; i++) {
                var currentBatch = Batches[i].Elements;

                for (int j = 0; j < currentBatch.Length; j++) {
                    var uiElement = currentBatch[j];
                    var uiEntity = conversionSystem.GetPrimaryEntity(uiElement);
                    renderEntities.Add(new RenderElement { Value = uiEntity });
                }

                batchSpans.Add(new int2(startIndex, currentBatch.Length));
                startIndex += currentBatch.Length;
            }

            // Build the material property batch by only taking the first element of the BatchedElements.
            // This is due to the first elementing being representative of the entire batch.
            var propertyBatch = new MaterialPropertyBatch {
                Value = new MaterialPropertyBlock[Batches.Length]
            };

            for (int i = 0; i < Batches.Length; i++) {
                var block = new MaterialPropertyBlock();

                if (Batches[i].Elements[0].TryGetComponent(out Image image)) {
                    var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                    block.SetTexture(ShaderIDConstants.MainTex, texture);

                    for (int k = 0; k < Batches[i].Elements.Length; k++) {
                        var associativeEntity = conversionSystem.GetPrimaryEntity(Batches[i].Elements[k]);
                        dstManager.AddComponentData(associativeEntity, new MaterialPropertyIndex { Value = (ushort)i });
                    }
                }

                propertyBatch.Value[i] = new MaterialPropertyBlock();
            }

            dstManager.AddComponentData(entity, propertyBatch);

            unsafe {
                var renderBatches = dstManager.AddBuffer<RenderElement>(entity);
                var size          = UnsafeUtility.SizeOf<RenderElement>() * renderEntities.Length;

                renderBatches.ResizeUninitialized(renderEntities.Length);
                UnsafeUtility.MemCpy(renderBatches.GetUnsafePtr(), renderEntities.GetUnsafePtr(), size);

                var renderSpans = dstManager.AddBuffer<BatchedSpanElement>(entity);
                size            = UnsafeUtility.SizeOf<BatchedSpanElement>() * batchSpans.Length;

                renderSpans.ResizeUninitialized(batchSpans.Length);
                UnsafeUtility.MemCpy(renderSpans.GetUnsafePtr(), batchSpans.GetUnsafePtr(), size);
            }

            renderEntities.Dispose();
            batchSpans.Dispose();
        }
    }
}
