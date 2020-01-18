using System.Collections;
using System.Runtime.InteropServices;
using UGUIDots.Render;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGeneration : MonoBehaviour {

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData {
        public Vector3 Position;
        // public Vector3 Normal;
        // public Color32 Color;
    }

    public OrthographicRenderFeature Feature;

    const int VertexCount = 4;
    const int IndexCount = 6;

    Mesh mesh;
    MaterialPropertyBlock block;
    Material mat;

    IEnumerator Start()
    {
        // mesh     = new Mesh();
        mesh     = MeshUtils.CreateQuad(100, 100);
        mat      = Canvas.GetDefaultCanvasMaterial();
        block    = new MaterialPropertyBlock {};

        block.SetColor(ShaderIDConstants.Color, Color.green);

        var width  = 100f;
        var height = 100f;
        var offset = 50;


        var vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0),
            new Vector3(width, 0, 0),

            new Vector3(width + offset, 0, 0),
            new Vector3(width + offset, height, 0),
            new Vector3(2 * width + offset, height, 0),
            new Vector3(2 * width + offset, 0, 0)
        };

        mesh.SetVertexBufferParams(vertices.Length, 
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32));

        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length, 0);

        var indices = new uint[] {
            0, 1, 2,
            0, 2, 3,

            4, 5, 6,
            4, 6, 7
        };

        mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Length);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length, MeshTopology.Triangles));
        // mesh.SetSubMesh(1, new SubMeshDescriptor(5, 6, MeshTopology.Triangles));
        mesh.UploadMeshData(false);

        var sub = mesh.GetSubMesh(0);
        Debug.Log($"Zeroth: Bounds: {sub.bounds}, Vertex Count: {sub.vertexCount}, First: {sub.firstVertex}");

        yield return null;
    }

    void Update()
    {
        var m = Matrix4x4.TRS(default, Quaternion.identity, Vector3.one);
        Feature.Pass.InstructionQueue.Enqueue((mesh, mat, m, block));
    }
}

