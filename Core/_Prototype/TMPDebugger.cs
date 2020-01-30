using System.Collections;
using System.Collections.Generic;
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
            var fontSize  = text.fontSize;
            var faceInfo  = fontAsset.faceInfo;

            var fontScale = fontSize / (float)faceInfo.pointSize;

            var parentScale  = transform.root.localScale;
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

            var ascent = faceInfo.ascentLine * fontScale;
            Vector2 start = default;
            Vector2 ascentLine = default;

            {
                var al = localTL - new Vector2(0, ascent);
                var ar = localTR - new Vector2(0, ascent);

                ascentLine = al;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(al), localToWorld.MultiplyPoint3x4(ar));

                if (text.alignment == TextAlignmentOptions.TopLeft) 
                {
                    start = al;
                }
            }

            // Draw the font lines
            {
                var c     = text.text[0];
                var glyph = fontAsset.glyphLookupTable[(uint)c];

                float width  = glyph.glyphRect.width;
                float height = glyph.glyphRect.height;

                var normalStyle = fontAsset.normalStyle;

                Debug.Log($"For {c}, bearing x: {glyph.metrics.horizontalBearingX}, y: {glyph.metrics.horizontalBearingY}, ");

                var xPos = (start.x + glyph.metrics.horizontalBearingX - normalStyle) * fontScale * parentScale.x;
                var yPos = (start.y - (height - glyph.metrics.horizontalBearingY - normalStyle)) * fontScale * parentScale.y;

                width  *= fontScale * parentScale.x;
                height = (height + normalStyle * 2) + fontScale * parentScale.y;

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
