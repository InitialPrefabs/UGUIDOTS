using UnityEngine;

namespace UGUIDots.Collections.Runtime {

    /// <summary>
    /// Stores textures that need to be referenced for issuing draw commands.
    /// </summary>
    [CreateAssetMenu(menuName = "UGUIDots/TextureBin", fileName = "TextureBin")]
    public class TextureBin : Bin<Texture> {

        public override void Prune(params int[] indices) {
            for (int i = 0; i < indices.Length; i++) {
                collection.RemoveAt(indices[i]);
            }
        }

        public override void Prune(params Texture[] values) {
            foreach (var texture in values) {
                collection.Remove(texture);
            }
        }
    }
}
