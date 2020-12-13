using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UGUIDOTS.Render.Authoring;

namespace UGUIDOTS.Conversions.Analyzers {

    internal static class BatchAnalysis {

        internal enum RenderedType {
            Image,
            Text,
            TextDynamic
        }

        internal struct RenderedElement {
            internal GameObject GameObject;
            internal RenderedType Type;
        }

        /// <summary>
        /// Creates a 2D collection of each submesh for the Canvas.
        /// </summary>
        internal static List<List<RenderedElement>> BuildStaticBatch(Canvas root) {
            Assert.IsNull(root.transform.parent, $"The current Canvas: {root.name} is not a root canvas!");
            var batchMap = new Dictionary<int, List<RenderedElement>>();
            RecurseChildrenBatch(root.transform, batchMap);

            // TODO: Clean up this very hacky way to separate the data - won't scale well.
            var collection            = new List<List<RenderedElement>>();
            var textCollection        = new List<List<RenderedElement>>();
            var dynamicTextCollection = new List<List<RenderedElement>>();
            foreach (var batch in batchMap.Values) {
                var first = batch[0];

                switch (first.Type) {
                    case RenderedType.Image:
                        collection.Add(batch);
                        break;
                    case RenderedType.Text:
                        textCollection.Add(batch);
                        break;
                    case RenderedType.TextDynamic:
                        dynamicTextCollection.Add(batch);
                        break;
                }
            }

            // TODO: Sort the collection so that all dynamic text is at the end.
            collection.AddRange(textCollection);
            collection.AddRange(dynamicTextCollection);
            return collection;
        }

        private static void RecurseChildrenBatch(Transform parent, Dictionary<int, List<RenderedElement>> batchMap) {
            if (parent.childCount == 0) {
                return;
            }

            for (int i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out Image img)) {
                    if (img.type == Image.Type.Simple) {
                        var texture  = img.sprite != null ? img.sprite.texture : (Texture)Texture2D.whiteTexture;
                        var material = img.material != null ? img.material : Canvas.GetDefaultCanvasMaterial();
                        var hash     = texture.GetHashCode() ^ material.GetHashCode();

                        if (!batchMap.TryGetValue(hash, out var collection)) {
                            collection = new List<RenderedElement>();
                            batchMap.Add(hash, collection);
                        }

                        var renderedElement = new RenderedElement {
                            GameObject      = child.gameObject,
                            Type            = RenderedType.Image
                        };

                        collection.Add(renderedElement);
                    } else if (img.type == Image.Type.Filled) {
                        var collection = new List<RenderedElement>();

                        collection.Add(new RenderedElement {
                            GameObject = child.gameObject,
                            Type       = RenderedType.Image
                        });
                        batchMap.Add(child.GetInstanceID(), collection);
                    }
                }

                // Get the authoring component which marks the text as dynamic.
                if (child.TryGetComponent(out TextMeshProUGUI text)) {
                    var hash = text.materialForRendering.GetHashCode();

                    var type = RenderedType.Text;

                    // Separate dynamic text from static text
                    if (child.TryGetComponent(out IAuthorableText dynamic)) {
                        hash ^= dynamic.GetHashCode();
                        type = RenderedType.TextDynamic;
                    }

                    if (!batchMap.TryGetValue(hash, out var collection)) {
                        collection = new List<RenderedElement>();
                        batchMap.Add(hash, collection);
                    }

                    var renderedElement = new RenderedElement {
                        GameObject      = text.gameObject,
                        Type            = type
                    };

                    collection.Add(renderedElement);
                }

                RecurseChildrenBatch(child, batchMap);
            }
        }
    }
}
