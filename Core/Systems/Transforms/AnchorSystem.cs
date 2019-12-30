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
                    /*
                    var dir = newPos - ltw.Position.xy;

                    Debug.Log($" Scale: {ltw.Scale()}, newPos: {newPos}, dir: {dir}");

                    var m = ltw.Value;
                    m.c3 = new float4(newPos, 0, 1);

                    var dest = Translations[current].Value + new float3(dir, 0);
                    CmdBuffer.SetComponent(i, current, new Translation { Value = dest });
                    */

                    var imParentLTW    = LTW[Parents[current].Value];
                    var inversedParent = math.inverse(imParentLTW.Value);
                    var newM           = float4x4.TRS(new float3(newPos, 0), default, new float3(1));
                    var localSpace     = new LocalToParent { Value = math.mul(inversedParent, newM) };

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

        private unsafe struct UpdateResolution : IJob {

            public int2 CurrentResolution;
            [NativeDisableUnsafePtrRestriction] public int2* Resolution;

            public void Execute() {
                *Resolution = CurrentResolution;
            }
        }

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery anchorQuery;

        protected override void OnCreate() {
            anchorQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadWrite<LocalToWorld>(),
                    ComponentType.ReadOnly<Anchor>(),
                    ComponentType.ReadOnly<Parent>()
                },
            });

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<ResolutionChangeEvt>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            var current = new int2(Screen.width, Screen.height);

            var anchorDeps   = new RecomputeAnchorJob {
                Resolution   = current,
                LTW          = GetComponentDataFromEntity<LocalToWorld>(true),
                AnchorType   = GetArchetypeChunkComponentType<Anchor>(true),
                Parents      = GetComponentDataFromEntity<Parent>(true),
                EntityType   = GetArchetypeChunkEntityType(),
                CmdBuffer    = cmdBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Translations = GetComponentDataFromEntity<Translation>(true)
            }.Schedule(anchorQuery, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(anchorDeps);

            return anchorDeps;
        }
    }
}
