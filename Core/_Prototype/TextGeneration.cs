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

        Material = Canvas.GetDefaultCanvasMaterial();

        mesh = MeshUtils.Experimental.CreateQuad(300, 300);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mesh.UploadMeshData(false);
    }

    void OnDrawGizmos() {
        if (Text.Length == 0) {
            return;
        }
        RenderTextQuads(Screen.width / 4, Screen.height / 2, 1);
    }

    void Update() {
        var m = Matrix4x4.TRS(new Vector3(Screen.width / 2, Screen.height / 2, 0), Quaternion.identity, Vector3.one);
        Feature.Pass.InstructionQueue.Enqueue((mesh, Material, m, block));
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

            // Debug.Log($"RenderTextQuads: {c}, Bearing Y: {glyph.BearingY(0)}, Bearing X: {glyph.BearingX()}, w: {glyph.Width()}, h: {glyph.Height()}");

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
}
