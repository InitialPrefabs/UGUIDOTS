using UnityEditor;
using UnityEngine;

namespace UGUIDOTS.EditorTools {

    public enum FillType {
        Axis = 0,
        Radial = 1
    }

    public enum Axis {
        X = 0,
        Y = 1
    }

    public class DefaultImageEditor : ShaderGUI {

        private MaterialProperty[] properties;
        private Object[] materials;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
            // base.OnGUI(materialEditor, properties);
            materials = materialEditor.targets;
            this.properties = properties;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fill Options", EditorStyles.boldLabel);

            if (!DrawToggleFillFeature()) {
                return;
            }

            EditorGUI.indentLevel++;

            switch (DrawFillTypeEnum()) {
                case FillType.Axis:
                    DrawAxis();
                    DrawAxisFill();
                    break;
                case FillType.Radial:
                    DrawRadialFill();
                    break;
            }

            EditorGUI.indentLevel--;
        }

        private bool DrawToggleFillFeature() {
            var toggleFill = FindProperty("_ToggleFill", properties);
            var isFilled = EditorGUILayout.Toggle(new GUIContent("Toggle Fill"), toggleFill.floatValue > 0);
            toggleFill.floatValue = isFilled ? 1 : 0;
            SetKeyword("_FILL", isFilled);

            return isFilled;
        }

        private FillType DrawFillTypeEnum() {
            var fillType = FindProperty("_FillType", properties);
            var fillFlag = (FillType)(int)fillType.floatValue;
            var adjustedFlag = (int)(FillType)EditorGUILayout.EnumPopup("Fill Type", fillFlag);
            fillType.floatValue = adjustedFlag;

            return (FillType)adjustedFlag;
        }

        private void DrawAxis() {
            var axis = FindProperty("_Axis", properties);
            var currentAxis = (Axis)(int)axis.floatValue;
            var adjustedAxis = (int)(Axis)EditorGUILayout.EnumPopup("Axis", currentAxis);
            axis.floatValue = adjustedAxis;
        }

        private void DrawAxisFill() {
            var flipFill = FindProperty("_Flip", properties);
            var isFlipped = EditorGUILayout.Toggle("Flip Axis", flipFill.floatValue > 0);
            flipFill.floatValue = isFlipped ? 1 : 0;

            var fillAmount = FindProperty("_Fill", properties);
            var adjustedFill = EditorGUILayout.Slider("Fill Amount", fillAmount.floatValue, 0f, 1f);
            fillAmount.floatValue = adjustedFill;
        }

        private void DrawRadialFill() {
            var angle = FindProperty("_Angle", properties);
            angle.floatValue = EditorGUILayout.Slider("Angle", angle.floatValue, 0f, 360f);

            var arc1 = FindProperty("_Arc1", properties);
            arc1.floatValue = EditorGUILayout.Slider("Arc 1", arc1.floatValue, 0f, 360f);

            var arc2 = FindProperty("_Arc2", properties);
            arc2.floatValue = EditorGUILayout.Slider("Arc 2", arc2.floatValue, 0f, 360f);
        }

        private void SetKeyword(string keyword, bool enabled) {
            if (enabled) {
                foreach (Material material in materials) {
                    material.EnableKeyword(keyword);
                }
            } else {
                foreach (Material material in  materials) {
                    material.DisableKeyword(keyword);
                }
            }
        }
    }
}
