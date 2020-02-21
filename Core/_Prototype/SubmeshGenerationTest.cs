using System.Collections.Generic;
using UGUIDots.Render;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SubmeshGenerationTest : MonoBehaviour {

    public OrthographicRenderFeature RenderFeature;
    public Vector2                   Size;
    public Material[]                Mats;

    List<LocalVertexData>       vertices = new List<LocalVertexData>();
    List<LocalTriangleIndexElement> indices  = new List<LocalTriangleIndexElement>();

    MaterialPropertyBlock block;
    Mesh                  mesh;

    void OnDrawGizmos() {

        vertices.Clear();
        indices .Clear();

        if (RenderFeature == null) {
            return;
        }

        if (mesh == null) {
            mesh = new Mesh();
        }

        if (block == null) {
            block = new MaterialPropertyBlock();
        }

        // Draw 3 square meshes that are batched together
        var extents = Size / 2f;

        var bl = -extents;
        var tl = new Vector2(-extents.x, extents.y);
        var tr = extents;
        var br = new Vector2(extents.x, -extents.y);

        var normal = new float3(1, 0, 0);
        var red = new float4(1, 0, 0, 1);

        vertices.Add(new LocalVertexData {
            Position = new float3(bl, 0),
            Normal   = normal,
            Color    = red
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tl, 0),
            Normal   = normal,
            Color    = red
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tr, 0),
            Normal   = normal,
            Color    = red
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(br, 0),
            Normal   = normal,
            Color    = red
        });

        indices.Add(0);
        indices.Add(1);
        indices.Add(2);
        indices.Add(0);
        indices.Add(2);
        indices.Add(3);

        var green = new float4(0, 1, 0, 1);

        vertices.Add(new LocalVertexData {
            Position = new float3(bl - new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = green
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tl - new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = green
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tr - new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = green
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(br - new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = green
        });

        indices.Add(4);
        indices.Add(5);
        indices.Add(6);
        indices.Add(4);
        indices.Add(6);
        indices.Add(7);

        var blue = new float4(0, 0, 1, 1);

        vertices.Add(new LocalVertexData {
            Position = new float3(bl + new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = blue
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tl + new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = blue
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tr + new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = blue
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(br + new Vector2(0, Size.y), 0),
            Normal   = normal,
            Color    = blue
        });

        indices.Add(8);
        indices.Add(9);
        indices.Add(10);
        indices.Add(8);
        indices.Add(10);
        indices.Add(11);

        var orange = new float4(1, 0.75f, 0, 1f);

        // New submesh
        vertices.Add(new LocalVertexData {
            Position = new float3(bl + Size, 0),
            Normal   = normal,
            Color    = orange
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tl + Size, 0),
            Normal   = normal,
            Color    = orange
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(tr + Size, 0),
            Normal   = normal,
            Color    = orange
        });

        vertices.Add(new LocalVertexData {
            Position = new float3(br + Size, 0),
            Normal   = normal,
            Color    = orange
        });

        indices.Add(12);
        indices.Add(13);
        indices.Add(14);
        indices.Add(12);
        indices.Add(14);
        indices.Add(15);

        mesh.SetVertexBufferParams(vertices.Count, MeshVertexDataExtensions.VertexDescriptors);
        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Count);
        mesh.SetIndexBufferParams(indices.Count, IndexFormat.UInt16);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Count);

        mesh.subMeshCount = 2;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Count - 6, MeshTopology.Triangles));

        Debug.Log($"{indices.Count - 6}");

        mesh.SetSubMesh(1, new SubMeshDescriptor {
            baseVertex  = 0,
            bounds      = default,
            firstVertex = vertices.Count - 4,
            indexCount  = 6,
            indexStart  = indices.Count - 6,
            topology    = MeshTopology.Triangles,
            vertexCount = 4
        });
        mesh.UploadMeshData(false);

        var m = Matrix4x4.TRS(new Vector3(Screen.width / 2f, Screen.height / 2f, 0), Quaternion.identity, Vector3.one);

        for (int i = 0; i < mesh.subMeshCount; i++) {
            RenderFeature.Pass.InstructionQueue.Enqueue((mesh, Mats[i], m, block));
        }
    }
}
