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

    [CustomEditor(typeof(BakedCanvasDataProxy))]
    public class BakedCanvasRunnerEditor : Editor {

        private enum ButtonState {
            Fail = -1,
            None = 0,
            Success = 1,
        }

        private BakedCanvasDataProxy canvasRunner;

        private void OnEnable() {
            canvasRunner = target as BakedCanvasDataProxy;
        }

        public override void OnInspectorGUI() {
            using (var changeCheck = new EditorGUI.ChangeCheckScope()) {
                serializedObject.Update();

                DefaultInspector();
                DrawBakeButton(out var state);

                if ((int)state > 0) {
                    EditorGUILayout.HelpBox("The element was successfully cached into the Baked Canvas Data", MessageType.Info);
                }

                if ((int)state < 0) {
                    EditorGUILayout.HelpBox("The element was not cached into the Baked Canvas Data!", MessageType.Error);
                }

                if ((int)state == 0) {
                    EditorGUILayout.HelpBox("Please ensure the element was cached into the Baked Canvas Data! " +
                        "Any structural changes will need to be rebaked.", MessageType.Warning);
                }

                UpdateIndexButton();
                
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

        private void DrawBakeButton(out ButtonState state) {
            if (GUILayout.Button("Bake Canvas Info")) {
                var canvasRoot = BuildHierarchy(canvasRunner.transform);

                var idxProp = serializedObject.FindProperty("Index");

                if (idxProp.intValue > -1) {
                    state = ButtonState.Fail;
                    return;
                }

                EditorUtility.SetDirty(canvasRunner.BakedCanvasData);

                canvasRunner.BakedCanvasData.Transforms.Add(canvasRoot);
                var idx = canvasRunner.BakedCanvasData.Transforms.Count - 1;

                idxProp.intValue = idx;
                state = ButtonState.None;

                return;
            }

            state = ButtonState.None;
        }

        private void UpdateIndexButton() {
            var idxProp = serializedObject.FindProperty("Index");
            if (GUILayout.Button("Update Baked Canvas Info") && idxProp.intValue > -1) {
                var canvasRoot = BuildHierarchy(canvasRunner.transform);
                EditorUtility.SetDirty(canvasRunner.BakedCanvasData);
                canvasRunner.BakedCanvasData.Transforms[idxProp.intValue] = canvasRoot;
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

        private void RecurseBuildHierarchy(Transform transform, CanvasTransform parent) {

            for (int i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);

                var childRect = child.GetComponent<RectTransform>();

                var worldPos   = childRect.position;
                var worldScale = childRect.lossyScale;
                var localPos   = childRect.localPosition;
                var localScale = childRect.localScale;

                var canvasTransform = new CanvasTransform(worldPos, worldScale, localPos, localScale);
                parent.Children.Add(canvasTransform);

                if (child.childCount > 0) {
                    RecurseBuildHierarchy(child, canvasTransform);
                }
            }
        }
    }
}