using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace UGUIDots {

    public class AscentLineDrawer : MonoBehaviour {
        public int PointSize = 90;

        FaceInfo faceInfo = default;

        Text text;

        void Start() {
            text = GetComponent<Text>();
            FontEngine.InitializeFontEngine();

            FontEngine.LoadFontFace(text.font, text.font.fontSize);
            faceInfo = FontEngine.GetFaceInfo();

            FontEngine.DestroyFontEngine();
        }

        void OnDrawGizmos() {

            if (text == null) { return; }

            var parentScale  = transform.root.localScale.x;
            Vector2 position = text.transform.position;
            var rect         = text.rectTransform.rect;
            var size         = rect.size;

            var localBL = -size / 2;
            var localTL = new Vector2(-size.x, size.y) / 2;
            var localTR = size / 2;
            var localBR = new Vector2(size.x, -size.y) / 2;

            var localToWorld = transform.localToWorldMatrix;

            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localBL), localToWorld.MultiplyPoint3x4(localBR));
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localBR), localToWorld.MultiplyPoint3x4(localTR));
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localTR), localToWorld.MultiplyPoint3x4(localTL));
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localBL), localToWorld.MultiplyPoint3x4(localTL));

            size = rect.size * parentScale;
            var fontScale = (float)text.fontSize / text.font.fontSize;
            var ascent = faceInfo.ascentLine * fontScale;

            Vector2 startPosition = default;

            {
                var al = localTL - new Vector2(0, ascent);
                var ar = localTR - new Vector2(0, ascent);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(al), localToWorld.MultiplyPoint3x4(ar));

                if (text.alignment == TextAnchor.UpperLeft) {
                    startPosition = al;
                }
            }

            {
                var avg = (faceInfo.lineHeight * fontScale) / 2 + (faceInfo.descentLine * fontScale);
                var al  = new Vector2(-size.x / 2, 0) - new Vector2(0, avg);
                var ar  = new Vector2(size.x / 2, 0) - new Vector2(0, avg);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(al), localToWorld.MultiplyPoint3x4(ar));

                if (text.alignment == TextAnchor.MiddleLeft) {
                    startPosition = al;
                }
            }

            {
                var initialChar = text.text[0];
                var charInfo = text.font.characterInfo;

                for (int i = 0; i < charInfo.Length; i++) {
                    if (charInfo[i].index == (int)initialChar) {
                        var canvasRoot = GetComponentInParent<Canvas>();
                        var canvasScale = canvasRoot.transform.localScale;

                        var current = charInfo[i];
                        var xPos = startPosition.x + current.bearing * canvasScale.x;
                        var yPos = startPosition.y - (current.glyphHeight - current.BearingY(0) * canvasScale.y);

                        var width  = current.Width() * canvasScale.x;
                        var height = current.Height() * canvasScale.y;

                        Debug.Log($"{text.font.name}, Font Size: {text.fontSize}, Char: {initialChar}, Width: {width}, Height: {height}");

                        var bl = localToWorld.MultiplyPoint3x4(new Vector2(xPos, yPos));
                        var tl = localToWorld.MultiplyPoint3x4(new Vector2(xPos, yPos + height));
                        var tr = localToWorld.MultiplyPoint3x4(new Vector2(xPos + width, yPos + height));
                        var br = localToWorld.MultiplyPoint3x4(new Vector2(xPos + width, yPos));

                        Debug.DrawLine(bl, tl, Color.yellow);
                        Debug.DrawLine(tl, tr, Color.yellow);
                        Debug.DrawLine(tr, br, Color.yellow);
                        Debug.DrawLine(bl, br, Color.yellow);
                    }
                }
            }
        }
    }
}
