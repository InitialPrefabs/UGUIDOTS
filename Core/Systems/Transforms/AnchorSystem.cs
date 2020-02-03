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
    [UpdateAfter(typeof(CanvasScalerSystem))]
    [UpdateInGroup(typeof(UITransformUpdateGroup))]
    public unsafe class AnchorSystem : JobComponentSystem {

        [BurstCompile]
        private struct RepositionToAnchorJob : IJobChunk {

            public int2 Resolution;

            [ReadOnly]
            public ArchetypeChunkEntityType EntityType;

            [ReadOnly]
            public BufferFromEntity<Child> ChildBuffers;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LTW;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToParent> LTP;

            [ReadOnly]
            public ComponentDataFromEntity<Anchor> Anchors;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> Parents;

            [ReadOnly]
            public ComponentDataFromEntity<Dimensions> Dimensions;

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

                    if (!Anchors.Exists(current)) { 
                        continue;
                    }

                    var anchor  = Anchors[current];

                    int2 worldAnchor;
                    if (Parents.Exists(parent)) {
                        // Since the parent exists, we want to adjust "texture" space to "local space" in accordance to
                        // the pivot.
                        var dimension       = Dimensions[parent];
                        var localResolution = dimension.Int2Size();
                        var localAnchor     = anchor.State.AnchoredTo(localResolution) - dimension.Int2Center();
                        var currentLTP      = LTP[current];
                        var localLTP        = float4x4.TRS(new float3(localAnchor, 0), currentLTP.LocalRotation(), currentLTP.Scale());
                        var anchorLTW       = math.mul(parentLTW.Value, localLTP);
                        worldAnchor         = new int2((int)anchorLTW.c3.x, (int)anchorLTW.c3.y) / (int)anchorLTW.c3.w;
                    } else {
                        var localResolution = Resolution;
                        worldAnchor = anchor.State.AnchoredTo(localResolution);
                    }

                    var distance   = anchor.Distance * initialScale;
                    var worldPos   = worldAnchor + distance;
                    var ltw        = LTW[current];
                    var m          = float4x4.TRS(new float3(worldPos, 0), ltw.Rotation, ltw.Scale());
                    var localSpace = new LocalToParent { Value = math.mul(parentInversed, m) };

                    CmdBuffer.SetComponent(current.Index, current, localSpace);
                    CmdBuffer.SetComponent(current.Index, current, new Translation { Value = localSpace.Position });

                    var adjustedScale = initialScale * ltw.Scale().xy;

                    if (ChildBuffers.Exists(current)) {
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
            var anchorDeps   = new RepositionToAnchorJob {
                Resolution   = new int2(Screen.width, Screen.height),
                LTW          = GetComponentDataFromEntity<LocalToWorld>(true),
                LTP          = GetComponentDataFromEntity<LocalToParent>(true),
                ChildBuffers = GetBufferFromEntity<Child>(true),
                Anchors      = GetComponentDataFromEntity<Anchor>(true),
                Parents      = GetComponentDataFromEntity<Parent>(true),
                Dimensions   = GetComponentDataFromEntity<Dimensions>(true),
                EntityType   = GetArchetypeChunkEntityType(),
                CmdBuffer    = cmdBufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(canvasQuery, inputDeps);

            cmdBufferSystem.AddJobHandleForProducer(anchorDeps);
            return anchorDeps;
        }
    }
}
