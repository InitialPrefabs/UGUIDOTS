using UnityEngine;
using TMPro;

namespace UGUIDots {

    public class TMPDebugger : MonoBehaviour {

        TMP_Text text;

        void OnDrawGizmos() {
            if (text == null) {
                text = GetComponent<TMP_Text>();
            }

            var fontAsset = text.font;
            var fontSize = text.fontSize;
            var faceInfo = fontAsset.faceInfo;

            var fontScale = fontSize / (float)faceInfo.pointSize;

            var canvas = GetComponentInParent<Canvas>();
            var parentScale = canvas.transform.localScale;

            Vector2 position = text.transform.position;
            var rect = text.rectTransform.rect;
            var size = rect.size;

            var localBL = -size / 2;
            var localTL = new Vector2(-size.x, size.y) / 2;
            var localTR = size / 2;
            var localBR = new Vector2(size.x, -size.y) / 2;

            var localToWorld = transform.localToWorldMatrix;

            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localBL), localToWorld.MultiplyPoint3x4(localBR));
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localBR), localToWorld.MultiplyPoint3x4(localTR));
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localTR), localToWorld.MultiplyPoint3x4(localTL));
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(localBL), localToWorld.MultiplyPoint3x4(localTL));

            var ascent = faceInfo.ascentLine * fontScale;
            Vector2 start = default;
            Vector2 ascentLine = default;

            {
                var al = localTL - new Vector2(0, ascent);
                var ar = localTR - new Vector2(0, ascent);

                ascentLine = al;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(al), localToWorld.MultiplyPoint3x4(ar));

                if (text.alignment == TextAlignmentOptions.TopLeft) {
                    start = al;
                }
            }

            var scale = new Vector2(parentScale.x, parentScale.y);

            if (scale.x > 1) {
                scale *= 0.5f;
            }

            if (scale.x < 1) {
                scale *= 2;
            }

            { // Draw the font lines
                for (int i = 0; i < text.text.Length; i++) {
                    var c = text.text[i];
                    var glyph = fontAsset.characterLookupTable[(uint)c];

                    var glyphRect = glyph.glyph.glyphRect;
                    var metrics = glyph.glyph.metrics;

                    float width = glyphRect.width;
                    float height = glyphRect.height;

                    var bearingUpL = localToWorld.MultiplyPoint3x4(new Vector2(localTL.x, start.y - (height - metrics.horizontalBearingY) * fontScale * scale.y));
                    var bearingUpR = localToWorld.MultiplyPoint3x4(new Vector2(localTR.x, start.y - (height - metrics.horizontalBearingY) * fontScale * scale.y));

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(bearingUpL, bearingUpR);

                    var xPos = start.x + (metrics.horizontalBearingX) * fontScale;
                    var yPos = start.y - ((height - metrics.horizontalBearingY) * fontScale);

                    width *= fontScale;
                    height *= fontScale;

                    var bl = localToWorld.MultiplyPoint3x4(new Vector2(xPos, yPos));
                    var tl = localToWorld.MultiplyPoint3x4(new Vector2(xPos, yPos + height));
                    var tr = localToWorld.MultiplyPoint3x4(new Vector2(xPos + width, yPos + height));
                    var br = localToWorld.MultiplyPoint3x4(new Vector2(xPos + width, yPos));

                    Debug.DrawLine(bl, tl, Color.yellow);
                    Debug.DrawLine(tl, tr, Color.yellow);
                    Debug.DrawLine(tr, br, Color.yellow);
                    Debug.DrawLine(bl, br, Color.yellow);

                    start += new Vector2(metrics.horizontalAdvance * fontScale, 0);
                }
            }
        }
    }
}
