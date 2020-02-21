using UGUIDots.Analyzers;
using UGUIDots.Render.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace UGUIDots.EditorTools {

    [CustomEditor(typeof(BatchedMeshAuthoring))]
    public class BatchedMeshAuthoringEditor : Editor {

        private SerializedProperty batchProperty;
        private BatchedMeshAuthoring batcher;

        private void OnEnable() {
            batchProperty = serializedObject.FindProperty("Batches");
            batcher = target as BatchedMeshAuthoring;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            DrawBatchLabel();
            DrawBatchButton();
        }

        private void DrawBatchLabel() {
            EditorGUILayout.LabelField($"There are currently {batcher.Batches.Length} batch(es).");
        }

        private void DrawBatchButton() {
            if (GUILayout.Button("Build Batch")) {
                serializedObject.Update();

                var canvas = batcher.GetComponent<Canvas>();
                Assert.IsNull(canvas.transform.parent, "Building a batch must only be done on the root gameObject!");
                var collection = BatchAnalysis.BuildStaticBatch(canvas);

                batchProperty.arraySize = collection.Count;

                for (int i = 0; i < batchProperty.arraySize; i++) {
                    var internalArray = batchProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Elements");
                    internalArray.arraySize = collection[i].Count;

                    for (int k = 0; k < collection[i].Count; k++) {
                        internalArray.GetArrayElementAtIndex(k).objectReferenceValue = collection[i][k];
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
