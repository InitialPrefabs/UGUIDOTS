using System.Runtime.CompilerServices;
using UGUIDOTS.Transforms;
using Unity.Collections;
using Unity.Entities;

namespace UGUIDOTS.Core.Reactive.Systems {

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    // TODO: For now signal using the OnResolutionChangeTag component.
    internal class SignalRebuildOnTextChangeSystem : SystemBase {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int HashChars(NativeArray<CharElement> chars) {
            var hash = 0;
            for (int i = 0; i < chars.Length; i++) {
                hash ^= chars[i].GetHashCode();
            }

            return hash;
        }

        private EntityQuery dynamicTextQuery;
        private NativeHashMap<Entity, int> hashes;
        private EntityCommandBufferSystem commandBufferSystem;
        
        protected override void OnCreate() {
            dynamicTextQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<CharElement>() }
            });

            commandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnDestroy() {
            if (hashes.IsCreated) {
                hashes.Dispose();
            }
        }

        protected override void OnUpdate() {
            var size = dynamicTextQuery.CalculateEntityCount();

            if (hashes.IsCreated) {
                TryResizeHashMap(size);
            } else {
                hashes = new NativeHashMap<Entity, int>(size * 2, Allocator.Persistent);
            }

            var hashesMap = hashes;
            var commandBuffer = commandBufferSystem.CreateCommandBuffer();

            // TODO: Add a system state component so that we can remove the entity from the hash map.
            Dependency = Entities.ForEach((
                Entity entity, in DynamicBuffer<CharElement> b0, in RootCanvasReference c0) => {

                var chars = b0.AsNativeArray();
                var hash = HashChars(chars);

                if (hashesMap.TryGetValue(entity, out int textHash)) {
                    if (textHash != hash) {
                        // TODO: Replace this with a system that only rebuilds dynamic text.
                        commandBuffer.AddComponent(c0.Value, new OnResolutionChangeTag());
                    }
                    hashesMap[entity] = hash;
                } else {
                    hashesMap.Add(entity, hash);
                }
            }).Schedule(Dependency);

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        private void TryResizeHashMap(int size) {
            if (hashes.Capacity < size) {
                hashes.Capacity = size;
            }
        }
    }
}
