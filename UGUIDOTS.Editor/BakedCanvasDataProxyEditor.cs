using System;
using UGUIDOTS.Analyzers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UGUIDOTS.EditorTools {

    [CustomEditor(typeof(BakedCanvasDataProxy))]
    public class BakedCanvasDataProxyEditor : Editor {

        const string BakeWarningMessage = "The Canvas's transform hierarchy has not yet been baked.";

        private BakedCanvasDataProxy proxy;
        private SerializedProperty instanceIDProp;
        private SerializedProperty bakedCanvasDataProp;
        private VisualElement container;

        private bool exists;

        private void OnEnable() {
            proxy = target as BakedCanvasDataProxy;
            instanceIDProp = serializedObject.FindProperty("instanceID");
            bakedCanvasDataProp = serializedObject.FindProperty("BakedCanvasData");

            container = new VisualElement();

            AssetDatabaseUtility.FindStyleSheet("Labels.uss", out var labelStyle);
            container.styleSheets.Add(labelStyle);
        }

        public override VisualElement CreateInspectorGUI() {
            container.Clear();

            serializedObject.Update();
            exists = Exists(out int instanceID);

            // TODO: Add a note about baking the information with the reference resolution.
            // TODO: Add an update button.
            DrawBakedCanvasDataField(container);
            DrawDisabledInstanceID(container, exists, instanceID);
            DrawBakeButton(container, exists, instanceID);
            RemoveBakedCanvasButton(container, instanceID);

            return container;
        }

        private void DrawDisabledInstanceID(VisualElement root, bool exists, int instanceID) {
            var prop = new PropertyField(instanceIDProp);
            prop.Bind(serializedObject);
            prop.schedule.Execute(() => {
                prop.ElementAt(0).Query<Label>().First().AddToClassList(StylesReference.LimitWidth40);
            });
            prop.SetEnabled(false);
            root.Add(prop);

            var helpBox = new HelpBox(BakeWarningMessage, HelpBoxMessageType.Warning) { name = "warning-msg" };
            if (exists) {
                helpBox.AddToClassList(StylesReference.Hidden);
            }

            root.Add(helpBox);
        }

        private void DrawBakedCanvasDataField(VisualElement root) {
            var canvasField = new PropertyField(bakedCanvasDataProp);
            container.schedule.Execute(() => {
                var helpBox = canvasField.ElementAt(0).Query<Label>().First();
                helpBox.AddToClassList(StylesReference.LimitWidth40);
            });
            root.Add(canvasField);
        }

        private void DrawBakeButton(VisualElement root, bool exists, int instanceID) {
            var bakeButton = new Button() {
                text = "Bake Canvas Hierarchy",
                name = "bake-button"
            };

            bakeButton.clicked += () => {
                serializedObject.Update();
                var canvasRoot = BuildHierarchy(proxy.transform, instanceID);
                instanceIDProp.intValue = instanceID;

                var canvasData = proxy.BakedCanvasData;
                // NOTE: Mark the BakedCanvasData as dirty so we  can apply and save changes to it.
                EditorUtility.SetDirty(canvasData);
                canvasData.Hierarchy.Add(canvasRoot);
                serializedObject.ApplyModifiedProperties();

                bakeButton.SetEnabled(false);
                CleanUp();
            };

            bakeButton.SetEnabled(!exists);
            root.Add(bakeButton);
        }

        private void RemoveBakedCanvasButton(VisualElement root, int instanceID) {
            var removeButton = new Button() {
                text = "Remove Baked Canvas",
                name = "remove-button"
            };

            removeButton.clicked += () => {
                serializedObject.Update();

                var canvasData = proxy.BakedCanvasData;
                EditorUtility.SetDirty(canvasData);
                canvasData.Hierarchy.RemoveAll((element) => {
                    return element.InstanceID == instanceID;
                });

                instanceIDProp.intValue = 0;

                root.Query<HelpBox>().ForEach(helpbox => {
                    helpbox.RemoveFromClassList(StylesReference.Hidden);
                });

                root.Query<Button>("bake-button").First().SetEnabled(true);

                exists = false;
                serializedObject.ApplyModifiedProperties();
            };

            root.Add(removeButton);
        }

        private void CleanUp() {
            container.Query<HelpBox>().ForEach(helpbox => helpbox.AddToClassList(StylesReference.Hidden));
        }

        private RootCanvasTransform BuildHierarchy(Transform transform, int instanceID) {
            if (transform.parent != null) {
                throw new NotSupportedException($"The transform: {transform.name} is not a root element to scan!");
            }

            var root = new RootCanvasTransform(
                transform.position, 
                transform.lossyScale, 
                transform.localPosition, 
                transform.localScale,
                instanceID,
                transform.name);

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

                var canvasTransform = new CanvasTransform(worldPos, worldScale, localPos, localScale, child.name);
                parent.Children.Add(canvasTransform);

                if (child.childCount > 0) {
                    RecurseBuildHierarchy(child, canvasTransform);
                }
            }
        }

        private bool InstanceIDExists() {
            var instanceID = instanceIDProp.intValue;
            var bakedCanvasData = proxy.BakedCanvasData;

            return bakedCanvasData.Hierarchy.Exists((element) => {
                return element.InstanceID == instanceID && element.InstanceID != 0;
            });
        }

        private bool Exists(out int instanceID) {
            if (InstanceIDExists()) {
                instanceID = instanceIDProp.intValue;
                return true;
            } else {
                instanceID = proxy.GetInstanceID();
                return false;
            }
        }
    }
}
