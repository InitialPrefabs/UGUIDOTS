using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

            public EntityCommandBuffer.Concurrent CmdBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);
                var anchors  = chunk.GetNativeArray(AnchorType);

                for (int i = 0; i < chunk.Count; i++) {
                    var current = entities[i];
                    var anchor  = anchors[i];
                    var ltw     = LTW[current];
                    var adjustedAnchors = AdjustAnchorsWithScale(current, anchor.Distance, in ltw);

                    Debug.Log($"Adjusted Distance: {adjustedAnchors.Distance} | {adjustedAnchors.LTW.Position}");

                    var anchoredRefPos = anchor.State.AnchoredTo(Resolution);
                    var newPos = anchoredRefPos + adjustedAnchors.Distance;

                    Debug.Log($" Scale: {ltw.Scale()}");

                    /*
                    CmdBuffer.SetComponent(i, current, new LocalToWorld {

                    });
                    */
                }
            }

            private AnchorInfo AdjustAnchorsWithScale(Entity e, float2 distance, in LocalToWorld ltw) {
                if (!Parents.Exists(e)) {
                    return new AnchorInfo {
                        Distance = distance,
                        LTW      = ltw,
                    };
                } 

                var parent       = Parents[e].Value;
                var parentLTW    = LTW[parent];
                var parentScale  = parentLTW.Scale();
                distance        *= parentScale.xy;

                return AdjustAnchorsWithScale(parent, distance, parentLTW);
            }
        }

        private unsafe struct UpdateResolution : IJob {

            public int2 CurrentResolution;
            [NativeDisableUnsafePtrRestriction] public int2* Resolution;

            public void Execute() {
                *Resolution = CurrentResolution;
            }
        }

        private EntityQuery anchorQuery;

        // TODO: Remove the pointer...don't think I need this anymore...
        private int2* res;

        protected override void OnCreate() {
            anchorQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadWrite<LocalToWorld>(),
                    ComponentType.ReadOnly<Anchor>(),
                    ComponentType.ReadOnly<Parent>()
                },
            });

            res = (int2*)UnsafeUtility.Malloc(sizeof(int2), sizeof(int2), Allocator.Persistent);
            *res = new int2(Screen.width, Screen.height);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var current = new int2(Screen.width, Screen.height);

            if (!res->Equals(current)) {
                // TODO: Need to change this because this might not scale well...? 
                inputDeps.Complete();
                var anchorDeps = new RecomputeAnchorJob {
                    Resolution = current,
                    LTW        = GetComponentDataFromEntity<LocalToWorld>(true),
                    AnchorType = GetArchetypeChunkComponentType<Anchor>(true),
                    Parents    = GetComponentDataFromEntity<Parent>(true),
                    EntityType = GetArchetypeChunkEntityType()
                }.Schedule(anchorQuery, inputDeps);

                return new UpdateResolution {
                    Resolution        = res,
                    CurrentResolution = current
                }.Schedule(anchorDeps);
            }
            
            return inputDeps;
        }
    }
}
