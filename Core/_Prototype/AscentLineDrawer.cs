using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public class AscentLineDrawer : MonoBehaviour
{
    public int PointSize = 90;

    FaceInfo faceInfo;
    void OnDrawGizmos() 
    {
        var text = GetComponent<Text>();
        FontEngine.InitializeFontEngine();

        FontEngine.LoadFontFace(text.font, text.font.fontSize);
        faceInfo = FontEngine.GetFaceInfo();

        FontEngine.DestroyFontEngine();

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

        {
            var al = localTL - new Vector2(0, ascent);
            var ar = localTR - new Vector2(0, ascent);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(al), localToWorld.MultiplyPoint3x4(ar));
        }

        {
            var avgLineHeight = (faceInfo.lineHeight * fontScale) / 2 + (faceInfo.descentLine * fontScale);

            var al = new Vector2(-size.x / 2, 0) - new Vector2(0, avgLineHeight);
            var ar = new Vector2(size.x / 2, 0) - new Vector2(0, avgLineHeight);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(localToWorld.MultiplyPoint3x4(al), localToWorld.MultiplyPoint3x4(ar));
        }
    }
}
