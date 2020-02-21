using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace UGUIDots.Analyzers {

    public static class BatchAnalysis {

        public static List<List<GameObject>> BuildStaticBatch(Canvas root) {
            Assert.IsNull(root.transform.parent, $"The current Canvas: {root.name} is not a root canvas!");
            var batchMap = new Dictionary<int, List<GameObject>>();
            RecurseChildrenBatch(root.transform, batchMap);

            var collection = new List<List<GameObject>>();
            foreach (var batch in batchMap.Values) {
                collection.Add(batch);
            }
            return collection;
        }

        private static void RecurseChildrenBatch(Transform parent, Dictionary<int, List<GameObject>> batchMap) {
            if (parent.childCount <= 0) {
                return;
            }

            for (int i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out Image img)) {
                    var texture  = img.sprite != null ? img.sprite.texture : (Texture)Texture2D.whiteTexture;
                    var material = img.material != null ? img.material : Canvas.GetDefaultCanvasMaterial();
                    var hash     = texture.GetHashCode() ^ material.GetHashCode();

                    if (!batchMap.TryGetValue(hash, out var collection)) {
                        collection = new List<GameObject>();
                        batchMap.Add(hash, collection);
                    }
                    collection.Add(child.gameObject);
                }

                if (child.TryGetComponent(out TextMeshProUGUI text)) {
                    var hash = text.materialForRendering != null ? text.materialForRendering.GetHashCode() : 0;

                    if (!batchMap.TryGetValue(hash, out var collection)) {
                        collection = new List<GameObject>();
                        batchMap.Add(hash, collection);
                    }
                    collection.Add(text.gameObject);
                }

                RecurseChildrenBatch(child, batchMap);
            }
        }
    }
}
