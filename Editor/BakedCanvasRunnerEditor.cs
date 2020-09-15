using System;
using UGUIDOTS.Analyzers;
using UnityEditor;
using UnityEngine;

using static UGUIDOTS.Analyzers.BakedCanvasData;

namespace UGUIDOTS.EditorTools {

    [CustomEditor(typeof(BakedCanvasData))]
    public class BakedCanvasDataEditor : Editor {

        public override void OnInspectorGUI() {
            HelpMessage();

            DrawDefaultInspector();
        }

        private void HelpMessage() {
            EditorGUILayout.HelpBox("Baked Canvas Data will store the transform data of all elements in the canvas. " + 
                "This is due to that baked data in subscenes are not reliable with Unity's Legacy UI.", 
                MessageType.Info);

            EditorGUILayout.HelpBox("Manipulating data directly on this Scriptable Object is not recommended!", 
                MessageType.Warning);
        }
    }

    [CustomEditor(typeof(BakedCanvasRunner))]
    public class BakedCanvasRunnerEditor : Editor {

        private BakedCanvasRunner canvasRunner;

        private void OnEnable() {
            canvasRunner = target as BakedCanvasRunner;
        }

        public override void OnInspectorGUI() {
            DefaultInspector();

            using (var changeCheck = new EditorGUI.ChangeCheckScope()) {
                serializedObject.Update();

                DrawBakeButton();
                
                if (changeCheck.changed) {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DefaultInspector() {
            var it = serializedObject.GetIterator();
            it.Next(true);

            while (it.NextVisible(true)) {
                var enabled = !(it.name == "m_Script" || it.name == "Index");

                GUI.enabled = enabled;
                EditorGUILayout.PropertyField(it, new GUIContent(it.displayName));
                GUI.enabled = true;
            }
        }

        private void DrawBakeButton() {
            if (GUILayout.Button("Bake Canvas Info")) {
                var canvasRoot = BuildHierarchy(canvasRunner.transform);

                var idxProp = serializedObject.FindProperty("Index");

                if (idxProp.intValue > -1) {
                    return;
                }

                EditorUtility.SetDirty(canvasRunner.BakedCanvasData);

                canvasRunner.BakedCanvasData.Transforms.Add(canvasRoot);
                var idx = canvasRunner.BakedCanvasData.Transforms.Count - 1;

                idxProp.intValue = idx;
            }
        }

        private CanvasTransform BuildHierarchy(Transform transform) {
            if (transform.parent != null) {
                throw new NotSupportedException($"The transform: {transform.name} is not a root element to scan!");
            }

            var root = new CanvasTransform(
                transform.position, transform.lossyScale, 
                transform.localPosition, transform.localScale);

            RecurseBuildHierarchy(transform, root);

            return root;
        }

        private void RecurseBuildHierarchy(
            Transform transform, CanvasTransform parent) {

            for (int i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);

                var worldPos   = child.position;
                var worldScale = child.lossyScale;
                var localPos   = child.localPosition;
                var localScale = child.localScale;

                var canvasTransform = new CanvasTransform(worldPos, worldScale, localPos, localScale);
                parent.Children.Add(canvasTransform);

                if (child.childCount > 0) {
                    RecurseBuildHierarchy(child, canvasTransform);
                }
            }
        }
    }
}
