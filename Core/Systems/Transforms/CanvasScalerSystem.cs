using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Transforms.Systems {

    [UpdateInGroup(typeof(UITransformConsumerGroup))]
    public class ConsumeChangeEvtSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;

        protected override void OnCreate() {
            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer().ToConcurrent();

            Dependency = Entities.ForEach((Entity entity, in ResolutionChangeEvt c0) => {
                cmdBuffer.DestroyEntity(entity.Index, entity);
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }

    /// <summary>
    /// Scales all the canvases if the resolution of the window changes.
    /// </summary>
    [UpdateInGroup(typeof(UITransformProducerGroup))]
    public unsafe class CanvasScalerSystem : SystemBase {

        private EntityCommandBufferSystem cmdBufferSystem;
        private EntityQuery scaleQuery;
        private EntityArchetype evtArchetype;

        private int2* res;

        protected override void OnCreate() {
            scaleQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<ReferenceResolution>(),
                    ComponentType.ReadOnly<WidthHeightRatio>(),
                    ComponentType.ReadWrite<LocalToWorld>()
                }
            });

            evtArchetype = EntityManager.CreateArchetype(typeof(ResolutionChangeEvt));

            cmdBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();

            res = (int2*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int2>(), UnsafeUtility.AlignOf<int2>(), Allocator.Persistent); 
            *res = new int2(Screen.width, Screen.height);
        }

        protected override void OnDestroy() {
            if (res != null) {
                UnsafeUtility.Free(res, Allocator.Persistent);
                res = null;
            }
        }

        protected unsafe override void OnUpdate() {
            var current = new int2(Screen.width, Screen.height);

            if (res->Equals(current)) {
                return;
            }

            *res = current;
            int2* local = res;

            var cmdBuffer = cmdBufferSystem.CreateCommandBuffer();

            Dependency = Entities.ForEach((ref LocalToWorld c2, in ReferenceResolution c0, in WidthHeightRatio c1) => {
                var logWidth  = math.log2(local->x / c0.Value.x);
                var logHeight = math.log2(local->y / c0.Value.y);
                var avg       = math.lerp(logWidth, logHeight, c1.Value);
                var scale     = math.pow(2, avg);
                var center    = new float3(local->xy / 2, 0);

                c2 = new LocalToWorld { Value = float4x4.TRS(center, c2.Rotation, new float3(scale)) };
            }).WithNativeDisableUnsafePtrRestriction(local).Schedule(Dependency);

            var archetype = evtArchetype;

            Dependency = Job.WithCode(() => {
                cmdBuffer.CreateEntity(archetype);
            }).Schedule(Dependency);

            cmdBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
