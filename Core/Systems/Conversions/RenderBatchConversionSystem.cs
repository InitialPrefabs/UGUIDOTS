using System.Collections.Generic;
using UGUIDots.Render;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    // TODO: Figure out batching
    [UpdateAfter(typeof(ImageConversionSystem))]
    public class RenderBatchConversionSystem : GameObjectConversionSystem {

        protected override void OnCreate() {
            base.OnCreate();
        }

        protected override void OnUpdate() {
            var batchedMap = new Dictionary<Transform, List<Image>>();
            Entities.ForEach((Canvas canvas) => {
            });
        }

        private void RecurseChildren(Transform parent) {
            var entity        = GetPrimaryEntity(parent);
            var batchedBuffer = DstEntityManager.AddBuffer<BatchedRenderElement>(entity);

            for (int i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out Image img)) {
                }
            }
        }
    }
}
