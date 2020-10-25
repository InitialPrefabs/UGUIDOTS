using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDOTS {

    public struct RootCanvasReference : IComponentData {
        public Entity Value;

        public static RootCanvasReference Default() {
            return new RootCanvasReference { Value = Entity.Null };
        }
    }

    /// <summary>
    /// Stores data about each child's active state in an unsafe hash map.
    /// </summary>
    public struct ChildrenActiveMetadata : ISystemStateComponentData, IDisposable {
        public UnsafeHashMap<Entity, bool> Value;

        public void Dispose() {
            Value.Dispose();
        }

        [BurstDiscard]
        public void Dispose(JobHandle jobDeps) {
            Value.Dispose(jobDeps);
        }
    }

    /// <summary>
    /// If the canvas is set to the ScaleWithScreenSize, then this component should be attached to the Canvas component.
    /// </summary>
    public struct ReferenceResolution : IComponentData {
        public float2 Value;
        public float WidthHeightWeight;
    }

    /// <summary>
    /// Marks the Canvas root entity to rebuild its current mesh based on the vertex/index buffer.
    /// </summary>
    public struct BuildCanvasTag : IComponentData { }

    /// <summary>
    /// Marks the vertex/index buffers need to be copied into the mesh.
    /// </summary>
    public struct BatchCanvasTag : IComponentData { }

    public static class CanvasExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AspectRatio(this in ReferenceResolution value) {
            return value.Value.x / value.Value.y;
        }
    }
}
