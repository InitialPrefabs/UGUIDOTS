using System.Collections.Generic;
using UnityEngine;

namespace UGUIDots.Collections.Runtime {

    public abstract class Bin<T> : ScriptableObject where T : Object {

        [Tooltip("What is the initial capacity of values we want to initialize with?")]
        public int InitialCapacity = 20;

        protected List<T> collection;

        protected virtual void OnEnable() {
            collection = new List<T>(InitialCapacity);
        }

        /// <summary>
        /// Adds an element to the bin and returns the index of the element in the bin.
        /// </summary>
        /// <param name="value">The element to add within the bin.</param>
        /// <returns>The index of the element that's been added to the bin.</returns>
        public int Add(T value) {
            if (!collection.Exists(v => v.Equals(value))) {
                collection.Add(value);
                return collection.Count - 1;
            }
            return collection.IndexOf(value);
        }

        /// <summary>
        /// Returns the element at said index.
        /// </summary>
        /// <param name="index">The element at said index.</param>
        /// <returns>An element of type T</returns>
        public T At(int index) {
            if (index < 0 || index >= collection.Count) {
                throw new System.ArgumentOutOfRangeException($"{index} is out of range and must be between " + 
                    $"[0, {collection.Count - 1}]");
            }
            return collection[index];
        }

        /// <summary>
        /// Use this as a safety check to help determine if the path is available for loading.
        /// </summary>
        /// <param name="path">The path to load at the resources.</param>
        /// <param name="bin">The stored resource type to load.</param>
        /// <returns>True, if the stored resource was successfully loaded.<returns>
        public static bool TryLoadBin(string path, out Bin<T> bin) {
            bin = Resources.Load<Bin<T>>(path);
            return bin != null;
        }

        /// <summary>
        /// Removes all elements at said index.
        /// </summary>
        public abstract void Prune(params int[] indices);

        /// <summary>
        /// Removes a collection of all elements within the bin.
        /// </summary>
        public abstract void Prune(params T[] values);
    }

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
