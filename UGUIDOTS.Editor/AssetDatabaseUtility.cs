using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace UGUIDOTS.EditorTools {
    internal static class AssetDatabaseUtility {

        internal static bool FindStyleSheet(string name, out StyleSheet styleSheet) {
            var guids = AssetDatabase.FindAssets("t: StyleSheet");

            var regex = new Regex($@"/{name}$");

            for (int i = 0; i < guids.Length; i++) {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (regex.Match(path).Success) {
                    styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                    return true;
                }
            }

            styleSheet = null;
            return false;
        }
    }
}
