using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDots.Render {

    public static class MeshUtils {

        public static class Experimental {

            public static readonly VertexAttributeDescriptor[] Layout = new [] {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 2),
                new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.UNorm8, 4)
            };

            [StructLayoutAttribute(LayoutKind.Sequential)]
            public struct VertexData {
                public float3 Position;
                public short NormalX, NormalY;
                public Color32 Tangent;
            }

            public static Mesh CreateQuad(int height, int width) {
                var mesh = new Mesh();

                var vertexData = new NativeArray<VertexData>(4, Allocator.Persistent);
                vertexData[0]  = new VertexData {
                    Position   = new float3(-width / 2, -height / 2, 0),
                    NormalX    = 0,
                    NormalY    = 1,
                    Tangent    = Color.red
                };
                vertexData[1]  = new VertexData {
                    Position   = new float3(-width / 2, height / 2, 0),
                    NormalX    = 0,
                    NormalY    = 1,
                    Tangent    = Color.red
                };
                vertexData[2]  = new VertexData {
                    Position   = new float3(width / 2, height / 2, 0),
                    NormalX    = 0,
                    NormalY    = 1,
                    Tangent    = Color.red
                };
                vertexData[3]  = new VertexData {
                    Position   = new float3(width / 2, -height / 2, 0),
                    NormalX    = 0,
                    NormalY    = 1,
                    Tangent    = Color.red
                };

                mesh.SetVertexBufferParams(4, Layout);
                mesh.SetVertexBufferData(vertexData, 0, 0, vertexData.Length);

                var indexData = new NativeArray<int>(6, Allocator.Persistent);

                indexData[0] = 0;
                indexData[1] = 1;
                indexData[2] = 2;
                indexData[3] = 0;
                indexData[4] = 2;
                indexData[5] = 3;

                mesh.SetIndexBufferParams(6, IndexFormat.UInt32);
                mesh.SetIndexBufferData(indexData, 0, 0, indexData.Length);

                indexData.Dispose();
                vertexData.Dispose();
                return mesh;
            }
        }

        public static Mesh CreateQuad(int width, int height) {
            var mesh    = new Mesh();
            var verts   = new Vector3[4];
            var norms   = new Vector3[4];
            var uvs     = new Vector2[4];
            var indices = new int[6];

            verts[0] = new Vector3(-width / 2, -height / 2);
            verts[1] = new Vector3(-width / 2, height / 2, 0);
            verts[2] = new Vector3(width / 2, height / 2, 0);
            verts[3] = new Vector3(width / 2, -height / 2, 0);

            for (int i = 0; i < 4; i++) {
                norms[i] = Vector3.up;
            }

            uvs[0] = new Vector2();
            uvs[1] = new Vector2(0, 1);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(1, 0);

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            mesh.vertices  = verts;
            mesh.normals   = norms;
            mesh.triangles = indices;
            mesh.uv        = uvs;

            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
