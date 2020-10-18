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
            public ComponentDataFromEntity<Parent> Parents;

            public ComponentDataFromEntity<LocalSpace> LocalSpace;

            public ComponentDataFromEntity<ScreenSpace> ScreenSpace;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities        = chunk.GetNativeArray(EntityType);
                for (int i          = 0; i < chunk.Count; i++) {
                    var parent      = entities[i];
                    var screenSpace = ScreenSpace[parent];
                    var children    = Children[parent].AsNativeArray();

                    RecurseAnchor(children, screenSpace, parent);
                }
            }

            void RecurseAnchor(NativeArray<Child> children, ScreenSpace parentSpace, Entity parent) {
                var m_Inverse = math.inverse(parentSpace.AsMatrix());

                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;

                    if (!Anchors.HasComponent(current)) {
                        continue;
                    }

                    var anchor       = Anchors[current];
                    var dimneions    = Dimensions[parent];
                    var localSpace   = LocalSpace[current];
                    var screenSpace = ScreenSpace[current];

                    var anchoredPos   = GetAnchoredPosition(parent, parentSpace.Translation, parentSpace.Scale, anchor);
                    var adjustedSpace = anchoredPos + (anchor.Distance * parentSpace.Scale);

                    var mWorld        = float4x4.TRS(new float3(adjustedSpace, 0), quaternion.identity, new float3(screenSpace.Scale, 1));
                    var localToParent = math.mul(m_Inverse, mWorld);

                    localSpace = new LocalSpace {
                        Scale           = localToParent.Scale().xy,
                        Translation     = localToParent.Position().xy
                    };

                    screenSpace = new ScreenSpace {
                        Scale            = mWorld.Scale().xy,
                        Translation      = mWorld.Position().xy
                    };

                    LocalSpace[current]  = localSpace;
                    ScreenSpace[current] = screenSpace;

                    CommandBuffer.AddComponent<UpdateSliceTag>(current);;

                    if (Children.HasComponent(current)) {
                        RecurseAnchor(Children[current].AsNativeArray(), screenSpace, current);
                    }
                }
            }

            float2 GetAnchoredPosition(Entity parent, float2 parentLTW, float2 scale, Anchor anchor) {
                var isParentVisual = Parents.HasComponent(parent) && LinkedMaterials.HasComponent(parent);

                if (isParentVisual) {
                    var dimenions = Dimensions[parent].Value;
                    var relativeAnchor = anchor.State.AnchoredToRelative(dimenions) * scale;
                    return parentLTW + relativeAnchor;
                }

                return anchor.State.AnchoredTo(Resolution);
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
                Resolution      = new int2(Screen.width, Screen.height),
                CommandBuffer   = cmdBufferSystem.CreateCommandBuffer()
            };
            anchorJob.Run(canvasQuery);
        }
    }
}
