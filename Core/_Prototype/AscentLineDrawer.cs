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

        FontEngine.LoadFontFace(text.font, text.fontSize);
        faceInfo = FontEngine.GetFaceInfo();

        FontEngine.DestroyFontEngine();


        Vector2 position = text.transform.position;
        var rect = text.rectTransform.rect;
        var size = rect.size;

        var bl = position - size / 2;
        var tl = position + new Vector2(-size.x, size.y) / 2;
        var tr = position + size / 2;
        var br = position + new Vector2(size.x,-size.y) / 2;

        Gizmos.DrawLine(bl, tl);
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(bl, br);

        var ascent = faceInfo.ascentLine;

        var al = tl - new Vector2(0, ascent);
        var ar = tr - new Vector2(0, ascent);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(al, ar);
    }
}
