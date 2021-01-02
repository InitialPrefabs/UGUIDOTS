using UGUIDOTS.Analyzers;
using UnityEditor;

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
}
