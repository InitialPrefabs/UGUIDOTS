using System.Collections.Generic;
using TMPro;
using UGUIDots.Render;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore;

public class TextGeneration : MonoBehaviour {

    public OrthographicRenderFeature Feature;
    public TMP_FontAsset FontAsset;
    public string Text;

    Dictionary<uint, Glyph> glyphLookUp;

    MaterialPropertyBlock block;

    void Start() {
        block       = new MaterialPropertyBlock();

        glyphLookUp = FontAsset.glyphLookupTable;
        block.SetColor(ShaderIDConstants.Color, Color.green);
    }

    void Update() {
        if (Text.Length == 0) {
            return;
        }

        RenderText(Text, 0, 0, 1, 0);
    }

    void RenderText(in string txt, float x, float y, float scale, int startIndex) {
        for (int i = 0; i < txt.Length; i++) {
            var c = txt[i];

            var glyph = glyphLookUp[c];
            var metrics = glyph.metrics;

            var xPos = x + metrics.horizontalBearingX * scale;
            var yPos = y - (glyph.glyphRect.height - metrics.horizontalBearingY) * scale;

            var width  = glyph.glyphRect.width * scale;
            var height = glyph.glyphRect.height * scale;

            var vertices = new Vector3[]
            {
                new Vector3(-width / 2, -height / 2),
                new Vector3(-width / 2, height / 2, 0),
                new Vector3(width / 2, height / 2, 0),
                new Vector3(width / 2, -height / 2, 0)
            };

            var indices = new int[] {
                startIndex,
                startIndex + 1,
                startIndex + 2,
                startIndex,
                startIndex + 2,
                startIndex + 3
            };

            var normals = new Vector3[] {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up,
            };

            x += (glyph.metrics.horizontalAdvance * scale);

            var mesh       = new Mesh();
            mesh.vertices  = vertices;
            mesh.triangles = indices;
            mesh.normals   = normals;
            mesh.RecalculateBounds();

            var pos = new float3(Screen.width / 4 + ((glyph.metrics.horizontalAdvance + width)* i), Screen.height / 2, 0f);
            var m = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
            Feature.Pass.InstructionQueue.Enqueue((mesh, Canvas.GetDefaultCanvasMaterial(), m, block));
        }
    }
}
