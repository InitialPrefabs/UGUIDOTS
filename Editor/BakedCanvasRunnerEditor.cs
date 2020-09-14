using UGUIDOTS.Analyzers;
using UnityEditor;
using UnityEngine;

namespace UGUIDOTS.EditorTools {
    [CustomEditor(typeof(BakedCanvasRunner))]
    public class BakedCanvasRunnerEditor : Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            DrawBakeButton();
        }

        private void DrawBakeButton() {
            if (GUILayout.Button("Bake Canvas Info")) {
                var runner = target as BakedCanvasRunner;
                var instanceID = runner.gameObject.GetInstanceID();

                using (var changeCheck = new EditorGUI.ChangeCheckScope()) {

                    if (runner.BakedCanvasData.CanvasTransforms.Exists(canvasData => canvasData.InstanceID == instanceID)) {
                        return;
                    }

                    var bakedSerializedObject = new SerializedObject(runner.BakedCanvasData);
                    bakedSerializedObject.Update();
                    serializedObject.Update();

                    runner.BakedCanvasData.CanvasTransforms.Add(new BakedCanvasData.CanvasTransform {
                        InstanceID = runner.gameObject.GetInstanceID(),
                        Translation = runner.transform.position,
                        Scale = runner.transform.localScale
                    });

                    var index = runner.BakedCanvasData.CanvasTransforms.Count - 1;

                    var prop = serializedObject.FindProperty("Index");
                    prop.intValue = index;

                    bakedSerializedObject.ApplyModifiedProperties();

                    if (changeCheck.changed) {
                        serializedObject.ApplyModifiedProperties();
                    }

                }
            }
        }
    }
}
