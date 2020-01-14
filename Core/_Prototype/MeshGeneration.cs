using System.Collections;
using System.Runtime.InteropServices;
using UGUIDots.Render;
using Unity.Collections;
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

    NativeArray<uint> indices;
    NativeArray<VertexData> vertices;
    Mesh mesh;
    MaterialPropertyBlock block;
    Material mat;

    IEnumerator Start()
    {
        // mesh     = new Mesh();
        mesh     = MeshUtils.CreateQuad(100, 100);
        indices  = new NativeArray<uint>(IndexCount, Allocator.Persistent);
        vertices = new NativeArray<VertexData>(VertexCount, Allocator.Persistent);
        mat      = Canvas.GetDefaultCanvasMaterial();
        block    = new MaterialPropertyBlock {};

        block.SetColor(ShaderIDConstants.Color, Color.magenta);

        /*
        mesh.SetVertexBufferParams(VertexCount,
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0));
            // new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 0),
            // new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, 0));

        yield return null;

        vertices[0]  = new VertexData {
            Position = default,
            // Normal   = -Vector3.forward,
            // Color    = Color.red
        };
        vertices[1]  = new VertexData {
            Position = new Vector3(0, 0, 100),
            // Normal   = -Vector3.forward,
            // Color    = Color.blue
        };
        vertices[2]  = new VertexData {
            Position = new Vector3(100, 0, 100),
            // Normal   = -Vector3.forward,
            // Color    = Color.green
        };
        vertices[3]  = new VertexData {
            Position = new Vector3(100, 0, 0),
            // Normal   = -Vector3.forward,
            // Color    = Color.yellow
        };

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;

        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log($"Before: {vertices[i].Position}");
        }

        yield return null;

        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

        mesh.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);
        mesh.SetIndexBufferData(indices, 0, 0, IndexCount);

        mesh.SetSubMesh(0, new SubMeshDescriptor(0, VertexCount, MeshTopology.Quads));

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Debug.Log($"After: {mesh.vertices[i]}, Before: {vertices[i].Position}");
        }
        */

        var width = 100f;
        var height = 100f;

        mesh.SetVertexBufferParams(5, 
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32));

        mesh.SetVertexBufferData(new Vector3[] {
            new Vector3(-width / 2, -height / 2),
            new Vector3(-width / 2, height / 2, 0),
            new Vector3(width / 2, height / 2, 0),
            new Vector3(width / 2, -height / 2, 0),
            new Vector3(width, height / 2, 0)
        }, 0, 0, 5, 0);

        mesh.SetIndexBufferParams(9, IndexFormat.UInt32);

        mesh.SetIndexBufferData(new uint[] {
            0, 1, 2,
            0, 2, 3,
            2, 3, 4
        }, 0, 0, 9);

        mesh.UploadMeshData(false);
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, 6, MeshTopology.Triangles));
        mesh.SetSubMesh(1, new SubMeshDescriptor(5, 3, MeshTopology.Triangles));

        var sub = mesh.GetSubMesh(0);
        Debug.Log($"Bounds: {sub.bounds}, Vertex Count: {sub.vertexCount}, First: {sub.firstVertex}");

        yield return null;
    }

    void OnDisable() {
        indices.Dispose();
        vertices.Dispose();
    }

    void Update()
    {
        var m = Matrix4x4.TRS(default, Quaternion.identity, Vector3.one);
        Feature.Pass.InstructionQueue.Enqueue((mesh, mat, m, block));
    }
}

