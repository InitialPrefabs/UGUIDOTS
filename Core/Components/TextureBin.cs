using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UGUIDots {
    
    /// <summary>
    /// Stores textures that need to be referenced for issuing draw commands.
    /// </summary>
    [CreateAssetMenu(menuName = "UGUIDots/TextureBin", fileName = "TextureBin")]
    public class TextureBin : ScriptableObject {

        public int InitialCapacity = 20;

        private IList<Texture> textures;

        private void OnEnable() {
            textures = new List<Texture>(InitialCapacity);
        }

        public bool Exists(Texture texture) {
            for (int i = 0; i < textures.Count; i++) {
                if (textures[i] == texture) {
                    return true;
                }
            }
            return false;
        }

        public int Add(Texture texture) {
            if (!Exists(texture)) {
                textures.Add(texture);
            }
            return textures.IndexOf(texture);
        }

        public void Prune(in NativeArray<int> indices, bool disposeOnCompletion = true) {
            for (int i = 0; i < indices.Length; i++) {
                textures.RemoveAt(indices[i]);
            }

            if (disposeOnCompletion) {
                indices.Dispose();
            }
        }
    }
}
