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
            public ComponentDataFromEntity<Stretch> Streched;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            public ComponentDataFromEntity<LocalSpace> LocalSpace;

            public ComponentDataFromEntity<ScreenSpace> ScreenSpace;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities        = chunk.GetNativeArray(EntityType);
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

                    if (!Streched.HasComponent(current)) {
                        var anchor       = Anchors[current];
                        var newScreenPos = anchor.RelativeAnchorTo(Resolution);

                        localSpace.Translation = (newScreenPos - (Resolution / 2));

                        screenSpace.Translation = newScreenPos;
                        ScreenSpace[current]    = screenSpace;
                        LocalSpace[current]     = localSpace;
                    }

                    if (Children.HasComponent(current)) {
                        var grandChildren = Children[current].AsNativeArray().AsReadOnly();
                        RecurseChildren(grandChildren, screenSpace);
                    }

                    CommandBuffer.AddComponent<UpdateSliceTag>(current);
                }
            }

            // TODO: Add a level recurse.
            void RecurseChildren(NativeArray<Child>.ReadOnly children, ScreenSpace parentSpace) {
                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;

                    var screenSpace = ScreenSpace[current];
                    var localSpace  = LocalSpace[current];

                    // If the hierarchy depth is > 1 then the rule is similar to the the first level children if it is 
                    // just an empty gameobject. Otherwise we take relative offsets based on the parent's position.

                    // TODO: Any known translation on the value needs to be kept
                    if (Anchors.HasComponent(current)) {
                        var anchor      = Anchors[current];
                        screenSpace.Translation = (parentSpace.Translation + anchor.Offset * screenSpace.Scale);
                        localSpace.Translation = (screenSpace.Translation - parentSpace.Translation);

                        ScreenSpace[current] = screenSpace;
                        LocalSpace[current] = localSpace;
                    }

                    if (Children.HasComponent(current)) {
                        RecurseChildren(Children[current].AsNativeArray().AsReadOnly(), screenSpace);
                    }
                    CommandBuffer.AddComponent<UpdateSliceTag>(current);
                }
            }

            float2 GetAnchoredPosition(Entity current, float2 parentLTW, float2 scale, Anchor anchor) {
                // var isParentVisual = Parents.HasComponent(current) && LinkedMaterials.HasComponent(current);

                // if (isParentVisual) {
                //     var dimenions      = Dimensions[current].Value;
                //     var relativeAnchor = anchor.State.AnchoredToRelative(dimenions) * scale;
                //     return parentLTW + relativeAnchor;
                // }

                // return anchor.State.AnchoredToRelative(Resolution);
                return default;
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery canvasQuery;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<ReferenceResolution>(), ComponentType.ReadOnly<Child>(),
                    ComponentType.ReadOnly<ScreenSpace>()
                },
                None = new[] {
                    ComponentType.ReadOnly<Parent>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            // RequireSingletonForUpdate<ResolutionEvent>();
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
                Streched        = GetComponentDataFromEntity<Stretch>(),
                Resolution      = new int2(Screen.width, Screen.height),
                CommandBuffer   = cmdBufferSystem.CreateCommandBuffer()
            };
            anchorJob.Run(canvasQuery);
        }
    }
}
