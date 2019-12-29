using UnityEngine;

namespace UGUIDots {

    public static class MeshUtils {

        public static Mesh CreateQuad(int width, int height) {
            var mesh = new Mesh();
            var verts = new Vector3[4];
            var norms = new Vector3[4];
            var uvs = new Vector2[4];
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

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = indices;
            mesh.uv = uvs;

            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
