using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Core.Diagnostics.Systems {
    internal static class MeshUtils {
        internal static Mesh CreateQuad(int2 size, Color color) {
            var mesh = new Mesh();

            mesh.SetVertices(new Vector3[] {
                new float3(-size / 2, 0),
                new float3(size / 2 * new float2(-1, 1), 0),
                new float3(size / 2, 0),
                new float3(size / 2 * new float2(1, -1), 0)
            });

            mesh.SetColors(new Color[] {
                color, color, color, color
            });

            mesh.SetIndices(new int[] {
                0, 1, 2,
                0, 2, 3
            }, MeshTopology.Triangles, 0);

            return mesh;
        }
    }
}
