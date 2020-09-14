using UGUIDOTS.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDOTS.Render.Systems {
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class UpdateLocalMeshDataSystem : SystemBase {

        [BurstCompile]
        private struct UpdateLocalVertexJob : IJobChunk {

            public float3 Offset;

            public EntityCommandBuffer.ParallelWriter CmdBuffer;

            [ReadOnly]
            public ComponentTypeHandle<Disabled> DisabledType;

            [ReadOnly]
            public ComponentTypeHandle<EnableRenderingTag> EnableRenderingType;

            [ReadOnly]
            public ComponentTypeHandle<AppliedColor> AppliedColorType;

            [ReadOnly]
            public EntityTypeHandle EntityType;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            public BufferTypeHandle<LocalVertexData> LocalVertexType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var colors         = chunk.GetNativeArray(AppliedColorType);
                var vertexBuffers  = chunk.GetBufferAccessor(LocalVertexType);
                var entities       = chunk.GetNativeArray(EntityType);
                var isDisabled     = chunk.Has(DisabledType);
                var isNewlyEnabled = chunk.Has(EnableRenderingType);

                // If newly disabled
                if (isDisabled && !isNewlyEnabled) {
                    for (int i = 0; i < chunk.Count; i++) {
                        var entity   = entities[i];
                        var vertices = vertexBuffers[i].AsNativeArray();
                        var color    = colors[i].Value.ToNormalizedFloat4();

                        for (int m = 0; m < vertices.Length; m++) {
                            var cpy       = vertices[m];
                            cpy.Color     = default;
                            cpy.Position += Offset;
                            vertices[m]   = cpy;
                        }

                        var root = HierarchyUtils.GetRoot(entity, Parents);
                        CmdBuffer.AddComponent<DisableRenderingTag>(chunkIndex, root);
                    }
                }

                // If this has been rendered to begin with
                if (!isDisabled && !isNewlyEnabled) {
                    for (int i       = 0; i < chunk.Count; i++) {
                        var entity   = entities[i];
                        var vertices = vertexBuffers[i].AsNativeArray();
                        var color    = colors[i].Value.ToNormalizedFloat4();

                        for (int m = 0; m < vertices.Length; m++) {
                            var cpy     = vertices[m];
                            cpy.Color   = color;
                            vertices[m] = cpy;
                        }

                        var root = HierarchyUtils.GetRoot(entity, Parents);
                        CmdBuffer.AddComponent<UpdateVertexColorTag>(chunkIndex, root);
                    }
                }

                // If the chunk is newly renabled
                if (!isDisabled && isNewlyEnabled) {
                    for (int i = 0; i < chunk.Count; i++) {
                        var entity   = entities[i];
                        var vertices = vertexBuffers[i].AsNativeArray();
                        var color    = colors[i].Value.ToNormalizedFloat4();

                        for (int m = 0; m < vertices.Length; m++) {
                            var cpy       = vertices[m];
                            cpy.Color     = color;
                            cpy.Position -= Offset;
                            vertices[m]   = cpy;
                        }

                        var root = HierarchyUtils.GetRoot(entity, Parents);
                        CmdBuffer.AddComponent<UpdateVertexColorTag>(chunkIndex, root);
                    }
                }
            }
        }

        private EntityQuery childrenUIQuery;
        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            childrenUIQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<UpdateVertexColorTag>(), ComponentType.ReadOnly<MeshDataSpan>(),
                    ComponentType.ReadWrite<LocalVertexData>(), ComponentType.ReadOnly<AppliedColor>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();

            RequireForUpdate(childrenUIQuery);
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency              = new UpdateLocalVertexJob {
                Offset              = new float3(Screen.width, Screen.height, 0) * 2,
                CmdBuffer           = cmdBuffer,
                Parents             = GetComponentDataFromEntity<Parent>(),
                AppliedColorType    = GetComponentTypeHandle<AppliedColor>(true),
                LocalVertexType     = GetBufferTypeHandle<LocalVertexData>(false),
                EntityType          = GetEntityTypeHandle(),
                EnableRenderingType = GetComponentTypeHandle<EnableRenderingTag>(true),
                DisabledType        = GetComponentTypeHandle<Disabled>(true)
            }.ScheduleParallel(childrenUIQuery, Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
