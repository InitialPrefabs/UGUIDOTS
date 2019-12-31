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
    [UpdateAfter(typeof(CanvasScalerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public unsafe class AnchorSystem : JobComponentSystem {

        /*
        private struct RecomputeAnchorJob : IJobChunk {

            private struct AnchorInfo {
                public float2 Distance;
                public LocalToWorld LTW;
            }

            public int2 Resolution;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LTW;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public ArchetypeChunkComponentType<Anchor> AnchorType;

            [ReadOnly]
            public ComponentDataFromEntity<Translation> Translations;
            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);
                var anchors = chunk.GetNativeArray(AnchorType);

                for (int i = 0; i < chunk.Count; i++) {
                    var current = entities[i];
                    var anchor = anchors[i];
                    var ltw = LTW[current];
                    var adjustedAnchors = AdjustAnchorsWithScale(current, anchor.Distance, in ltw);

                    var anchoredRefPos = anchor.State.AnchoredTo(Resolution);
                    var newPos = anchoredRefPos + adjustedAnchors.Distance;

                    var imParentLTW = LTW[Parents[current].Value];
                    var inversedParent = math.inverse(imParentLTW.Value);
                    var newM = float4x4.TRS(new float3(newPos, 0), default, new float3(1));
                    var localSpace = new LocalToParent { Value = math.mul(inversedParent, newM) };

                    CmdBuffer.SetComponent(i, current, localSpace);
                    CmdBuffer.SetComponent(i, current, new Translation { Value = localSpace.Position });
                }
            }

            private AnchorInfo AdjustAnchorsWithScale(Entity e, float2 distance, in LocalToWorld ltw) {
                if (!Parents.Exists(e)) {
                    return new AnchorInfo {
                        Distance = distance,
                        LTW = ltw,
                    };
                }

                var parent = Parents[e].Value;
                var parentLTW = LTW[parent];
                var parentScale = parentLTW.Scale();
                distance *= parentScale.xy;

                return AdjustAnchorsWithScale(parent, distance, parentLTW);
            }
        }
        */

        private struct RepositionToAnchorJob : IJobChunk {

            public int2 Resolution;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public BufferFromEntity<Child> ChildBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LTW;

            [ReadOnly]
            public ComponentDataFromEntity<Anchor> Anchors;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public ComponentDataFromEntity<ImageDimensions> Dimensions;

            public EntityCommandBuffer.Concurrent CmdBuffer;

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

            private void RecurseChildren(in Entity parent, in LocalToWorld parentLTW, in float2 initialScale, 
                in DynamicBuffer<Child> children) {

                var parentInversed = math.inverse(parentLTW.Value);

                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;
                    var anchor  = Anchors[current];

                    // TODO: If under local resolution - convert it to world space somehow.
                    var distance        = anchor.Distance * initialScale;
                    var localResolution = Parents.Exists(parent) ? Dimensions[parent].Int2Size() : Resolution;
                    var anchoredRef     = anchor.State.AnchoredTo(localResolution);

                    var worldPos = anchoredRef + distance;
                    var ltw      = LTW[current];

                    Debug.Log($"World pos: {worldPos}");

                    var m          = float4x4.TRS(new float3(worldPos, 0), ltw.Rotation, ltw.Scale());
                    var localSpace = new LocalToParent { Value = math.mul(parentInversed, m) };

                    CmdBuffer.SetComponent(current.Index, current, localSpace);
                    CmdBuffer.SetComponent(current.Index, current, new Translation { Value = localSpace.Position });

                    var adjustedScale = initialScale * ltw.Scale().xy;

                    if (ChildBuffers.Exists(current)) {
                        Debug.Log("Entering child...");
                        var grandChildren = ChildBuffers[current];
                        RecurseChildren(in current, in ltw, in adjustedScale, in grandChildren);
                    }
                }
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

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var anchorDeps = new RepositionToAnchorJob {
                Resolution   = new int2(Screen.width, Screen.height),
                LTW          = GetComponentDataFromEntity<LocalToWorld>(true),
                ChildBuffers = GetBufferFromEntity<Child>(true),
                Anchors      = GetComponentDataFromEntity<Anchor>(true),
                Parents      = GetComponentDataFromEntity<Parent>(true),
                Dimensions   = GetComponentDataFromEntity<ImageDimensions>(true),
                EntityType   = GetArchetypeChunkEntityType(),
                CmdBuffer    = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(canvasQuery, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(anchorDeps);
            return anchorDeps;
        }
    }
}
