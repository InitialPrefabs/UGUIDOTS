using UnityEngine;

namespace UGUIDots.Collections.Runtime {

    /// <summary>
    /// Stores Materials used for quick and easy access.
    /// </summary>
    [CreateAssetMenu(menuName = "UGUIDots/MaterialBin", fileName = "MaterialBin")]
    public class MaterialBin : Bin<Material> {

        public override void Prune(params int[] indices) {
            for (int i = 0; i < indices.Length; i++) {
                var index = indices[i];

                if (index < 0 || index > collection.Count - 1) {
                    continue;
                }

                collection.RemoveAt(i);
            }
        }

        public override void Prune(params Material[] values) {
            for (int i = 0; i < values.Length; i++) {
                var value = values[i];
                collection.Remove(value);
            }
        }
    }
}
