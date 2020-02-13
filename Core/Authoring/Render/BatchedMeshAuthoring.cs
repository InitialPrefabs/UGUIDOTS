using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

            unsafe {
                var renderBatches      = dstManager.AddBuffer<RenderElement>(entity);
                var size               = UnsafeUtility.SizeOf<RenderElement>() * renderEntities.Length;

                renderBatches.ResizeUninitialized(renderEntities.Length);
                UnsafeUtility.MemCpy(renderBatches.GetUnsafePtr(), renderEntities.GetUnsafePtr(), size);

                var renderSpans      = dstManager.AddBuffer<BatchedSpanElement>(entity);
                size                 = UnsafeUtility.SizeOf<BatchedSpanElement>() * batchSpans.Length;

                renderSpans.ResizeUninitialized(batchSpans.Length);
                UnsafeUtility.MemCpy(renderSpans.GetUnsafePtr(), batchSpans.GetUnsafePtr(), size);
            }

            renderEntities.Dispose();
            batchSpans.Dispose();
        }
    }
}
