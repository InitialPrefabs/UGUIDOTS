using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGUIDots.Render.Systems {

    // I'm not entirely convinced this achitecture would work, it does not include batching of meshes which would be
    // important to reduce the # of issued draw calls to the GPU...
    [UpdateInGroup(typeof(MeshBatchingGroup))]
    [UpdateAfter(typeof(BuildImageVertexDataSystem))]
    public class MeshCacheSystem : ComponentSystem {

        public struct MeshPropertyPair : IEquatable<MeshPropertyPair> {
            public Mesh Mesh;
            public MaterialPropertyBlock PropertyBlock;

            public bool Equals(MeshPropertyPair other) {
                return other.Mesh == Mesh && other.PropertyBlock == PropertyBlock;
            }

            public override int GetHashCode() {
                return Mesh.GetHashCode() ^ PropertyBlock.GetHashCode();
            }
        }

        // TODO: Don't think this is well thought out - I think I can just get away with 2 lists, a free list and a
        // used list
        private IList<MeshPropertyPair> cached, free;

        private EntityQuery unmappedMeshQuery, purgeableQuery;

        protected override void OnCreate() {
            unmappedMeshQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<Dimensions>(), ComponentType.ReadOnly<MeshVertexData>(),
                    ComponentType.ReadOnly<TriangleIndexElement>()
                },
                None = new [] {
                    ComponentType.ReadOnly<MeshIndex>()
                }
            });
        }

        protected override void OnStartRunning() {
            var actualCount = unmappedMeshQuery.CalculateEntityCount();

            if (cached == null) {
                cached = new List<MeshPropertyPair>(actualCount);
            }

            if (free == null) {
                free = new List<MeshPropertyPair>(actualCount);
            }
        }

        protected override void OnUpdate() {
            var charBuffers = GetBufferFromEntity<CharElement>(true);
            Entities.With(unmappedMeshQuery).ForEach(
                (Entity e, DynamicBuffer<MeshVertexData> b0, DynamicBuffer<TriangleIndexElement> b1) => {
                var vertices = b0.AsNativeArray();
                var indices  = b1.AsNativeArray();

                var pair = TryGetFreeMesh();
                var mesh = pair.Mesh;
                mesh.SetVertexBufferParams(vertices.Length, MeshVertexDataExtensions.VertexDescriptors);
                mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length, 0);
                mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);
                mesh.SetIndexBufferData(indices, 0, 0, indices.Length);

                mesh.subMeshCount = 1;
                mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length, MeshTopology.Triangles));
                mesh.RecalculateBounds();

                mesh.UploadMeshData(false);

                cached.Add(pair);
                var meshIdx = cached.IndexOf(pair);

                var meshKey = new MeshIndex { Value = meshIdx };
                PostUpdateCommands.AddComponent(e, meshKey);
            });
        }

        private MeshPropertyPair TryGetFreeMesh() {
            if (free.Count > 0) {
                var last = free.Count - 1;
                var pair = free[last];

                free.RemoveAt(last);
                pair.Mesh.Clear();
                pair.PropertyBlock.Clear();
                return pair;
            }

            return new MeshPropertyPair {
                Mesh = new Mesh(),
                PropertyBlock = new MaterialPropertyBlock()
            };
        }

        /// <summary>
        /// Performs a swap from the internal cache list to the free mesh list, if the index is valid.
        /// </summary>
        public void Purge(int index) {
            if (cached.Count >= index) {
                return;
            }

            free.Add(cached[index]);
            cached.RemoveAt(index);
        }

        /// <summary>
        /// Attempts to grab the mesh at some index if available.
        /// </summary>
        public bool TryGetMeshPropertyAt(int index, out MeshPropertyPair pair) {
            if (index < cached.Count) {
                pair = cached[index];
                return true;
            }

            pair = default;
            return false;
        }
    }
}
