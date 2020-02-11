using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UGUIDots.Collections.Unsafe;
using Unity.Mathematics;
using System.Collections.Generic;

namespace UGUIDots.Render.Authoring {

    public class MeshBatchesAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        [System.Serializable]
        public struct RenderBatches {
            public GameObject[] Elements;
        }

        public RenderBatches[] Batches;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var spans = new UnsafeArray<int2>(Batches.Length, Allocator.Persistent);
            var batched = new List<Entity>();

            var startIndex = 0;
            for (int i = 0; i < Batches.Length; i++) {
                // Store the offset first and how many elements are in the batch
                var elements = Batches[i].Elements;
                spans[i] = new int2(startIndex, elements.Length);
                startIndex += elements.Length;

                foreach (var element in elements) {
                    var extraEntity = conversionSystem.GetPrimaryEntity(element);
                    batched.Add(extraEntity);
                }
            }

            var entities = new UnsafeArray<Entity>(batched.Count, Allocator.Persistent);
            for (int i = 0; i < entities.Length; i++) {
                entities[i] = batched[i];
            }

            dstManager.AddComponentData(entity, new MeshBatches {
                Elements = entities,
                Spans    = spans
            });
        }
    }
}
