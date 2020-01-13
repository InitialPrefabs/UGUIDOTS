using System.Collections.Generic;
using TMPro;
using UGUIDots;
using UGUIDots.Render;
using UnityEngine;
using UnityEngine.TextCore;

[ExecuteAlways]
public class TextGeneration : MonoBehaviour {

    public OrthographicRenderFeature Feature;
    public TMP_FontAsset FontAsset;
    public string Text;
    public Vector2 size;
    public Font FontToUse;
    public float Spacing = 1.0f;
    public Material Material;

    Dictionary<uint, Glyph> glyphLookUp;
    MaterialPropertyBlock block;
    Mesh mesh;

    UIVertex[] temp = new UIVertex[4];

    void Start() {
        block = new MaterialPropertyBlock();

        glyphLookUp = FontAsset.glyphLookupTable;
        block.SetColor(ShaderIDConstants.Color, Color.green);

        mesh = new Mesh();
    }

    void OnDrawGizmos() {
        if (Text.Length == 0) {
            return;
        }

        RenderTextQuads(Screen.width / 4, Screen.height / 2, 1);
        RenderText(Text, 0, 2, 1);
    }

    void RenderTextQuads(float x, float y, float scale)
    {
        for (int i = 0; i < Text.Length; i++)
        {
            var c = Text[i];
            FontToUse.GetCharacterInfo(c, out CharacterInfo glyph);

            var xPos = x + glyph.BearingX() * scale;
            var yPos = y - (glyph.Height() - glyph.BearingY(0)) * scale;

            var width = glyph.Width() * scale;
            var height = glyph.Height() * scale;

            var BL = new Vector3(xPos, yPos);
            var TL = new Vector3(xPos, yPos + height);
            var TR = new Vector3(xPos + width, yPos + height);
            var BR = new Vector3(xPos + width, yPos);

            Debug.Log($"RenderTextQuads: {c}, Bearing Y: {glyph.BearingY(0)}, Bearing X: {glyph.BearingX()}, w: {glyph.Width()}, h: {glyph.Height()}");

            /*
            var mesh = new Mesh();

            mesh.vertices = new Vector3[] 
            {
                BL,
                TL,
                TR,
                BR
            };

            mesh.normals = new Vector3[] 
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };

            mesh.triangles = new int[] {
                0, 1, 2,
                0, 2, 3
            };

            mesh.uv = new Vector2[] {
                glyph.uvBottomLeft,
                glyph.uvTopLeft,
                glyph.uvTopRight,
                glyph.uvBottomRight
            };

            mesh.RecalculateBounds();

            Feature.Pass.InstructionQueue.Enqueue((mesh, Material, transform.localToWorldMatrix, block));
            */

            x += (glyph.Advance() * Spacing) * scale;

            Gizmos.DrawLine(BL, TL);
            Gizmos.DrawLine(TL, TR);
            Gizmos.DrawLine(TR, BR);
            Gizmos.DrawLine(BL, BR);
        }
    }

    void RenderText(in string txt, float x, float y, float scale) {
        if (glyphLookUp == null) {
            Start();
        }

        for (int i = 0; i < txt.Length; i++) {
            var c = txt[i];

            var glyph   = glyphLookUp[c];
            var metrics = glyph.metrics;

            var bearingX = metrics.horizontalBearingX;
            var bearingY = metrics.horizontalBearingY;

            var advance = metrics.horizontalAdvance;

            var gWidth = metrics.width;
            var gHeight = metrics.height;

            var xPos = x + bearingX * scale;
            var yPos = y - (gHeight - bearingY) * scale;

            Debug.Log($"RenderText: {c} Bearing X: {bearingX} Bearing Y: {bearingY}, Width: {gWidth}, Height: {gHeight}");

            var w = gWidth * scale;
            var h = gHeight * scale;

            var bl = new Vector2(xPos, yPos);
            var br = new Vector2(xPos + w, yPos);
            var tl = new Vector2(xPos, yPos + h);
            var tr = new Vector2(xPos + w, yPos + h);

            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(bl, tl);
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);

            x += (advance * Spacing) * (scale);
        }
    }
}
