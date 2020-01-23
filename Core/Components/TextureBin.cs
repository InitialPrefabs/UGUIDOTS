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
            if (textures == null) {
                textures = new List<Texture>(InitialCapacity);
            }
        }

        public int Add(Texture texture) {
            if (!(textures as List<Texture>).Exists(t => t == texture)) {
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

        public Texture At(int index) {
            return textures[index];
        }

        public static bool TryLoadTextureBin(string path, out TextureBin textureBin) {
            textureBin = Resources.Load<TextureBin>(path);
            var isNull = textureBin == null;
#if UNITY_EDITOR
            if (isNull) {
                throw new System.NullReferenceException($"Project does not have a TextureBin at Assets/Resources/{path}!");
            }
#endif
            return !isNull;
        }
    }
}
