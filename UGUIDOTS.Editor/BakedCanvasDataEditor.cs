using UGUIDOTS.Analyzers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UGUIDOTS.EditorTools {

    [CustomEditor(typeof(BakedCanvasData))]
    public class BakedCanvasDataEditor : Editor {

        private SerializedProperty hierarchyProp;
        private bool editState;

        private void OnEnable() {
            hierarchyProp = serializedObject.FindProperty("Hierarchy");
        }

        public override VisualElement CreateInspectorGUI() {
            var container = new VisualElement();

            DrawHelpMessageBox(container);
            DrawEditToggleButton(container);
            DrawHierarchyField(container);

            return container;
        }

        private void DrawHelpMessageBox(VisualElement root) {
            var helpbox = new HelpBox("It's generally not recommended to manipulate this data directly.", 
                HelpBoxMessageType.Warning);

            root.Add(helpbox);
        }

        private void DrawEditToggleButton(VisualElement root) {
            var editButton = new Button() {
                text = "Edit Data [Off]",
                name = "edit-data"
            };

            editButton.clicked += () => {
                editState = !editState;
                root.Query<PropertyField>("hierarchy-field").First().SetEnabled(editState);
                editButton.text = $"Edit Data [{(editState ? "On" : "Off")}]";
            };

            root.Add(editButton);
        }

        private void DrawHierarchyField(VisualElement root) {
            var hierarchyField = new PropertyField(hierarchyProp);
            hierarchyField.name = "hierarchy-field";
            hierarchyField.BindProperty(hierarchyProp);
            hierarchyField.SetEnabled(false);
            root.Add(hierarchyField);
        }
    }
}
