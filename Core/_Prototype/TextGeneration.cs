using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UGUIDots;
using UGUIDots.Render;
using UnityEngine;
using UnityEngine.TextCore;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.TextCore.LowLevel;

public class TextGeneration : MonoBehaviour {

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData {
        public float3 Position;
        public float3 Normal;
        public float2 UV;
    }

    public OrthographicRenderFeature Feature;
    public TMP_FontAsset FontAsset;
    public string Text;
    public Vector2 size;
    public Font FontToUse;
    public float Spacing = 1.0f;
    public Material Material;

    [Header("Bounding Box")]
    public Vector2 Dimensions;

    List<VertexData> vertexInfo;
    List<uint> indices;
    Dictionary<uint, Glyph> glyphLookUp;
    MaterialPropertyBlock block;
    Mesh mesh;
    string _internal;

    FaceInfo faceInfo;

    void Start() {
        block = new MaterialPropertyBlock();

        // glyphLookUp = FontAsset.glyphLookupTable;
        // block.SetColor(ShaderIDConstants.Color, Color.green);

        // Material = Canvas.GetDefaultCanvasMaterial();
        mesh = new Mesh();

        vertexInfo = new List<VertexData>();
        indices = new List<uint>();

        FontEngine.InitializeFontEngine();

        FontEngine.LoadFontFace(FontToUse, 90);
        faceInfo = FontEngine.GetFaceInfo();

        // FontToUse.RequestCharactersInTexture(Text, 0, FontStyle.Italic | FontStyle.Normal | FontStyle.Bold | FontStyle.BoldAndItalic);
        FontToUse.RequestCharactersInTexture(Text);

        FontEngine.DestroyFontEngine();
    }

    void OnDraw() {
        var extents = Dimensions / 2;
        var origin = new Vector2(Screen.width, Screen.height) / 2;

        var bl = -extents + origin;
        var tl = new Vector2(-extents.x, extents.y) + origin;
        var tr = extents + origin;
        var br = new Vector2(extents.x, -extents.y) + origin;

        Debug.DrawLine(bl, tl);
        Debug.DrawLine(tl, tr);
        Debug.DrawLine(tr, br);
        Debug.DrawLine(bl, br);

        var ascentLineHeightY = tl.y - faceInfo.ascentLine;
        var aL = new Vector2(tl.x, ascentLineHeightY);
        var aR = new Vector2(tr.x, ascentLineHeightY);

        Debug.DrawLine(aL, aR, Color.cyan);

        if (mesh == null) {
            mesh = new Mesh();
        }

        if (vertexInfo == null) {
            vertexInfo = new List<VertexData>();
        }

        if (indices == null) {
            indices = new List<uint>();
        }

        if (Text != _internal) {
            mesh.Clear();
            vertexInfo.Clear();
            indices.Clear();
            RenderTextQuads(aL.x, aL.y, 2);
            _internal = Text;
            Debug.Log("Build");
        }

        var m = Matrix4x4.TRS(default, Quaternion.identity, Vector3.one);
        Feature.Pass.InstructionQueue.Enqueue((mesh, Material, m, block));
    }

    void Update() 
    {
        OnDraw();
    }

    /*
    void Update() {
        if (Text.Length == 0) {
            mesh.Clear();
            return;
        }

        if (Text != _internal) {
            mesh.Clear();
            vertexInfo.Clear();
            indices.Clear();
            RenderTextQuads(Screen.width / 2, Screen.height / 2, 2);
            _internal = Text;
        }

        var m = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
        Feature.Pass.InstructionQueue.Enqueue((mesh, Material, m, block));
    }
    */

    void RenderTextQuads(float x, float y, float scale)
    {
        for (int i = 0; i < Text.Length; i++)
        {
            var c = Text[i];
            FontToUse.GetCharacterInfo(c, out CharacterInfo glyph, 0, FontStyle.Normal);

            Debug.Log($"{(int)FontStyle.Normal}, {(int)FontStyle.Bold} {(int)FontStyle.Italic} {(int)FontStyle.BoldAndItalic}");
            Debug.Log(FontStyle.Normal | FontStyle.Bold);
            Debug.Log(glyph.style);

            var xPos = x + glyph.BearingX() * scale;
            var yPos = y - (glyph.Height() - glyph.BearingY(0)) * scale;

            var width = glyph.Width() * scale;
            var height = glyph.Height() * scale;

            var BL = new Vector3(xPos, yPos);
            var TL = new Vector3(xPos, yPos + height);
            var TR = new Vector3(xPos + width, yPos + height);
            var BR = new Vector3(xPos + width, yPos);

            vertexInfo.Add(new VertexData {
                Position = BL,
                Normal   = Vector3.right,
                UV       = glyph.uvBottomLeft
            });
            vertexInfo.Add(new VertexData {
                Position = TL,
                Normal   = Vector3.right,
                UV       = glyph.uvTopLeft
            });
            vertexInfo.Add(new VertexData {
                Position = TR,
                Normal   = Vector3.right,
                UV       = glyph.uvTopRight,
            });
            vertexInfo.Add(new VertexData {
                Position = BR,
                Normal   = Vector3.right,
                UV       = glyph.uvBottomRight
            });

            var baseIndex = (uint)vertexInfo.Count - 4;

            indices.AddRange(new uint[] {
                baseIndex, baseIndex + 1, baseIndex + 2,
                baseIndex, baseIndex + 2, baseIndex + 3
            });

            x += (glyph.Advance() * Spacing) * scale;
        }

        mesh.SetVertexBufferParams(vertexInfo.Count,
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));

        mesh.SetVertexBufferData(vertexInfo, 0, 0, vertexInfo.Count, 0);

        mesh.SetIndexBufferParams(indices.Count, IndexFormat.UInt32);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Count);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Count, MeshTopology.Triangles));
        mesh.UploadMeshData(false);
    }
}
