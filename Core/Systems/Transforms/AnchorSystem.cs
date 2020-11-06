using UGUIDOTS.Render;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UGUIDOTS.Transforms.Systems {

    /// <summary>
    /// Recomputes the anchors if the resolution changes.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public unsafe class AnchorSystem : SystemBase {

        [BurstCompile]
        struct AnchorJob : IJobChunk {

            public EntityCommandBuffer CommandBuffer;

            public int2 Resolution;

            [ReadOnly]
            public EntityTypeHandle EntityType;

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public ComponentDataFromEntity<LinkedMaterialEntity> LinkedMaterials;

            [ReadOnly]
            public ComponentDataFromEntity<Anchor> Anchors;

            [ReadOnly]
            public ComponentDataFromEntity<Dimension> Dimensions;

            [ReadOnly]
            public ComponentDataFromEntity<Stretch> Stretched;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            public ComponentDataFromEntity<LocalSpace> LocalSpace;

            public ComponentDataFromEntity<ScreenSpace> ScreenSpace;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities    = chunk.GetNativeArray(EntityType);

                for (int i          = 0; i < chunk.Count; i++) {
                    var parent      = entities[i];
                    var screenSpace = ScreenSpace[parent];
                    var children    = Children[parent].AsNativeArray();

                    UpdateFirstLevelChildren(children.AsReadOnly(), screenSpace);
                }
            }

            void UpdateFirstLevelChildren(NativeArray<Child>.ReadOnly rootChildren, ScreenSpace root) {
                for (int i = 0; i < rootChildren.Length; i++) {
                    var current = rootChildren[i].Value;

                    var screenSpace  = ScreenSpace[current];
                    var localSpace   = LocalSpace[current];

                    if (!Stretched.HasComponent(current)) {
                        var anchor       = Anchors[current];
                        var newScreenPos = anchor.RelativeAnchorTo(Resolution, root.Scale);

                        localSpace.Translation = (newScreenPos - (Resolution / 2)) / root.Scale;
                        screenSpace.Translation = newScreenPos;
                    } else {
                        localSpace.Translation = default;
                        screenSpace.Translation = Resolution / 2;
                    }

                    ScreenSpace[current] = screenSpace;
                    LocalSpace[current]  = localSpace;

                    if (Children.HasComponent(current)) {
                        var grandChildren = Children[current].AsNativeArray().AsReadOnly();
                        RecurseChildren(grandChildren, screenSpace, current, root.Scale);
                    }
                }
            }

            void RecurseChildren(NativeArray<Child>.ReadOnly children, ScreenSpace parentSpace, Entity parent, 
                float2 rootScale) {

                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;

                    var screenSpace = ScreenSpace[current];
                    var localSpace  = LocalSpace[current];

                    if (Anchors.HasComponent(current)) {
                        var anchor = Anchors[current];

                        var parentDim     = Dimensions[parent];
                        var adjustedWorld = anchor.RelativeAnchorTo(parentDim.Extents(), rootScale, parentSpace.Translation);

                        screenSpace.Translation = adjustedWorld;
                        localSpace.Translation = parentSpace.Translation - adjustedWorld;

                        ScreenSpace[current] = screenSpace;
                        LocalSpace[current]  = localSpace;
                    } else if (Stretched.HasComponent(current)) {
                        screenSpace.Translation = parentSpace.Translation;
                        localSpace.Translation = float2.zero;

                        ScreenSpace[current] = screenSpace;
                        LocalSpace[current] = localSpace;
                    }

                    if (Children.HasComponent(current)) {
                        var grandChildren = Children[current].AsNativeArray().AsReadOnly();
                        RecurseChildren(grandChildren, screenSpace, current, rootScale);
                    }
                    // CommandBuffer.AddComponent<UpdateSliceTag>(current);
                }
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery canvasQuery;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<ReferenceResolution>(), ComponentType.ReadOnly<Child>(),
                    ComponentType.ReadOnly<ScreenSpace>(), ComponentType.ReadOnly<OnResolutionChangeTag>()
                },
                None = new[] {
                    ComponentType.ReadOnly<Parent>()
                },
                Options = EntityQueryOptions.IncludeDisabled
            });

            cmdBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var anchorJob       = new AnchorJob {
                EntityType      = GetEntityTypeHandle(),
                Children        = GetBufferFromEntity<Child>(true),
                Anchors         = GetComponentDataFromEntity<Anchor>(true),
                Dimensions      = GetComponentDataFromEntity<Dimension>(true),
                LinkedMaterials = GetComponentDataFromEntity<LinkedMaterialEntity>(true),
                Parents         = GetComponentDataFromEntity<Parent>(true),
                LocalSpace      = GetComponentDataFromEntity<LocalSpace>(),
                ScreenSpace     = GetComponentDataFromEntity<ScreenSpace>(),
                Stretched        = GetComponentDataFromEntity<Stretch>(),
                Resolution      = new int2(Screen.width, Screen.height),
                CommandBuffer   = cmdBufferSystem.CreateCommandBuffer()
            };
            anchorJob.Run(canvasQuery);
        }
    }
}
