using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace UGUIDots {

    public class ImageDebugger : MonoBehaviour {
        void OnDrawGizmos() {
            var image = GetComponent<Image>();

            var rect = image.rectTransform.rect;

            var localToWorld = image.transform.localToWorldMatrix;
            var extents = rect.size / 2;

            var bl = new Vector2(-extents.x, -extents.y);
            var tl = bl + new Vector2(0, extents.y) * 2;
            var tr = bl + new Vector2(extents.x, extents.y) * 2;
            var br = bl + new Vector2(extents.x, 0) * 2;

            var wbl = localToWorld.MultiplyPoint3x4(bl);
            var wtl = localToWorld.MultiplyPoint3x4(tl);
            var wtr = localToWorld.MultiplyPoint3x4(tr);
            var wbr = localToWorld.MultiplyPoint3x4(br);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(wbl, wtl);
            Gizmos.DrawLine(wtl, wtr);
            Gizmos.DrawLine(wtr, wbr);
            Gizmos.DrawLine(wbr, wbl);

            { // Pixel adjust point
                var spriteW = Mathf.RoundToInt(rect.width);
                var spriteH = Mathf.RoundToInt(rect.height);

                var textureRes = new Vector2(image.sprite.texture.width, image.sprite.texture.height);
                var scale = rect.size / textureRes;

                var padding = DataUtility.GetPadding(image.sprite);

                var adjustedV = new Vector4(
                    padding.x / spriteW,
                    (padding.y * scale.y) / spriteH,
                    (spriteW - padding.z) / spriteW,
                    (spriteH - padding.w * scale.y) / spriteH
                );

                var v = new Vector4(
                    bl.x + rect.width * adjustedV.x,
                    bl.y + rect.height * adjustedV.y,
                    bl.x + rect.width * adjustedV.z,
                    bl.y + rect.height * adjustedV.w
                );

                var sBl = localToWorld.MultiplyPoint3x4(new Vector2(v.x, v.y + (scale.y * 1.5f)));
                var sTl = localToWorld.MultiplyPoint3x4(new Vector2(v.x, v.w - (scale.y * 1.5f)));
                var sTr = localToWorld.MultiplyPoint3x4(new Vector2(v.z, v.w - (scale.y * 1.5f)));
                var sBr = localToWorld.MultiplyPoint3x4(new Vector2(v.z, v.y + (scale.y * 1.5f)));

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(sBl, sTl);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(sTl, sTr);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(sTr, sBr);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(sBl, sBr);
            }
        }
    }
}
