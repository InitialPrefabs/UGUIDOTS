using UGUIDOTS.Render.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace UGUIDOTS.EditorTools {

    [System.Obsolete]
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
            var batches = batcher.Batches;
            EditorGUILayout.LabelField($"There are currently {(batches != null ? batches.Length : 0)} batch(es).");
        }

        private void DrawBatchButton() {
            if (GUILayout.Button("Build Batch")) {
                throw new System.NotImplementedException("This functionality is deprecated");
            }
        }
    }
}
