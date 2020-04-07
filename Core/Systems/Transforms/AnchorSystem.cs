using System.Runtime.CompilerServices;
using UGUIDots.Render;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    /// <summary>
    /// Recomputes the anchors if the resolution changes.
    /// </summary>
    [UpdateInGroup(typeof(UITransformUpdateGroup))]
    public unsafe class AnchorSystem : SystemBase {

        [BurstCompile]
        private struct RepositionToAnchorJob : IJobChunk {

            public int2 Resolution;

            public ComponentDataFromEntity<LocalToParent> LTP;
            public ComponentDataFromEntity<Translation> Translations;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LTW;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public BufferFromEntity<Child> ChildBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<Anchor> Anchors;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public ComponentDataFromEntity<Dimensions> Dimensions;

            [ReadOnly]
            public ComponentDataFromEntity<LinkedMaterialEntity> LinkedMaterials;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity = entities[i];
                    var ltw = LTW[entity];
                    var initialScale = ltw.Scale().xy;

                    var children = ChildBuffers[entity];
                    RecurseChildren(in entity, in ltw, in initialScale, in children);
                }
            }

            private void RecurseChildren(in Entity parent, in LocalToWorld parentLTW, in float2 rootScale, 
                in DynamicBuffer<Child> children) {

                var parentInversed = math.inverse(parentLTW.Value);

                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;

                    if (!Anchors.Exists(current)) { 
                        continue;
                    }

                    // Get the current anchor, dimensions, and transforms
                    var anchor     = Anchors[current];
                    var dimensions = Dimensions[parent].Value;
                    var ltp        = LTP[current];
                    var ltw        = LTW[current];

                    // Find the world space position of the anchor
                    var anchoredPos = GetAnchoredPosition(parent, in parentLTW, in rootScale, in anchor);

                    // Get the actual world space and compute the local space.
                    var adjustedWS    = anchoredPos + (anchor.Distance * rootScale);
                    var localDistance = new float3(parentLTW.Position.xy - adjustedWS, 0);
                    var mWorldSpace   = float4x4.TRS(new float3(adjustedWS, 0), ltw.Rotation, ltw.Scale());
                    var mLocalSpace   = new LocalToParent { Value = math.mul(parentInversed, mWorldSpace) };

                    // Update the LocalToParent and its local translation
                    LTP[current] = mLocalSpace;
                    Translations[current] = new Translation { Value = mLocalSpace.Position };

                    // TODO: Update the local to world?
                    ltw = new LocalToWorld { 
                        Value = float4x4.TRS(new float3(adjustedWS, 0), ltw.Rotation, ltw.Scale()) 
                    };

                    if (ChildBuffers.Exists(current)) {
                        var grandChildren = ChildBuffers[current];
                        RecurseChildren(in current, in ltw, in rootScale, in grandChildren);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private float2 GetAnchoredPosition(Entity parent, in LocalToWorld parentLTW, in float2 scale, in Anchor anchor) {
                var isParentVisual = Parents.Exists(parent) && LinkedMaterials.Exists(parent);

                if (isParentVisual) {
                    // Get the local space of the dimensions we're working with in screen space.
                    // So w/ a 100x100 dimension, the mid left will give 0, 50.
                    var dimensions     = Dimensions[parent].Value;
                    var relativeAnchor = anchor.State.AnchoredToRelative(dimensions) * scale;

                    // Get the parent's matrix
                    return parentLTW.Position.xy + relativeAnchor;
                }

                // The default is a position returned in world space.
                return anchor.State.AnchoredTo(Resolution);
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery anchorQuery, canvasQuery;

        protected override void OnCreate() {
            anchorQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadWrite<LocalToWorld>(),
                    ComponentType.ReadOnly<Anchor>(),
                    ComponentType.ReadOnly<Parent>()
                },
            });

            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<ReferenceResolution>(),
                    ComponentType.ReadOnly<Child>()
                },
                None = new[] {
                    ComponentType.ReadOnly<Parent>()
                }
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<ResolutionChangeEvt>();
        }

        protected override void OnUpdate() {
            new RepositionToAnchorJob {
                Resolution      = new int2(Screen.width, Screen.height),
                Translations    = GetComponentDataFromEntity<Translation>(),
                LTP             = GetComponentDataFromEntity<LocalToParent>(),
                LTW             = GetComponentDataFromEntity<LocalToWorld>(true),
                ChildBuffers    = GetBufferFromEntity<Child>(true),
                Anchors         = GetComponentDataFromEntity<Anchor>(true),
                Parents         = GetComponentDataFromEntity<Parent>(true),
                Dimensions      = GetComponentDataFromEntity<Dimensions>(true),
                EntityType      = GetArchetypeChunkEntityType(),
                LinkedMaterials = GetComponentDataFromEntity<LinkedMaterialEntity>(true),
            }.Run(canvasQuery);
        }
    }
}
