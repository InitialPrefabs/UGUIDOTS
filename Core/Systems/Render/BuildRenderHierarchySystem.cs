using UGUIDOTS.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace UGUIDOTS.Render.Systems {

    // TODO: Switch to a multithreaded system.
    /**
     * Maybe per canvas, grab all of the Images and Text entities. Build all static content first.
     */
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class BuildRenderHierarchySystem : SystemBase {

        [BurstCompile]
        struct CollectEntitiesJob : IJobChunk {

            [ReadOnly]
            public BufferFromEntity<Child> Children;

            [ReadOnly]
            public EntityTypeHandle EntityType;

            // Images
            // -----------------------------------------
            [ReadOnly]
            public ComponentDataFromEntity<SpriteData> SpriteData;

            [ReadOnly]
            public ComponentDataFromEntity<Stretch> Stretched;

            // Text
            // -----------------------------------------
            [ReadOnly]
            public BufferFromEntity<CharElement> CharBuffers;

            // TODO: When parallelizing this, best to have per thread containers.
            [WriteOnly]
            public NativeList<Entity> SimpleImgEntities;

            [WriteOnly]
            public NativeList<Entity> StretchedImgEntities;

            [WriteOnly]
            public NativeList<Entity> TextEntities;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity = entities[i];
                    var children = Children[entity].AsNativeArray().AsReadOnly();

                    RecurseChildrenDetermineType(children, entity);
                }
            }

            void RecurseChildrenDetermineType(NativeArray<Child>.ReadOnly children, Entity root) {
                for (int i = 0; i < children.Length; i++) {
                    var entity = children[i];
                    if (SpriteData.HasComponent(entity)) {
                        if (Stretched.HasComponent(entity)) {
                            StretchedImgEntities.Add(entity);
                        } else {
                            SimpleImgEntities.Add(entity);
                        }
                    }

                    if (CharBuffers.HasComponent(entity)) {
                        TextEntities.Add(entity);
                    }

                    if (Children.HasComponent(entity)) {
                        var grandChildren = Children[entity].AsNativeArray().AsReadOnly();
                        RecurseChildrenDetermineType(grandChildren, root);
                    }
                }
            }
        }

        // NOTE: Assume all static
        struct BuildSimpleImgVertexDataJob : IJob {

            public EntityCommandBuffer CommandBuffer;

            public BufferFromEntity<Vertex> VertexData;

            [ReadOnly]
            public NativeList<Entity> Images;

            [ReadOnly]
            public ComponentDataFromEntity<SpriteData> SpriteData;

            [ReadOnly]
            public ComponentDataFromEntity<DefaultSpriteResolution> SpriteResolutions;

            [ReadOnly]
            public ComponentDataFromEntity<Dimension> Dimensions;

            [ReadOnly]
            public ComponentDataFromEntity<AppliedColor> Colors;

            [ReadOnly]
            public ComponentDataFromEntity<ScreenSpace> ScreenSpaces;

            [ReadOnly]
            public ComponentDataFromEntity<MeshDataSpan> MeshDataSpans;

            [ReadOnly]
            public ComponentDataFromEntity<RootCanvasReference> Root;

            public void Execute() {
                var tempImageData = new NativeArray<Vertex>(4, Allocator.Temp);

                for (int i = 0; i < Images.Length; i++) {
                    var entity = Images[i];
                    var root = Root[entity].Value;

                    // Get the root data
                    var vertices      = VertexData[root];
                    var rootTransform = ScreenSpaces[root];

                    // Build the image data
                    var spriteData  = SpriteData[entity];
                    var resolution  = SpriteResolutions[entity];
                    var dimension   = Dimensions[entity];
                    var screenSpace = ScreenSpaces[entity];
                    var color       = Colors[entity].Value.ToNormalizedFloat4();

                    var minMax = ImageUtils.CreateImagePositionData(resolution, spriteData, dimension, screenSpace, 
                        rootTransform.Scale.x);

                    var span = MeshDataSpans[entity];

                    // if (span.VertexSpan.Equals(new int2(0, 4)) && color.Equals(new float4(1, 0, 0, 1))) {
                    //     UnityEngine.Debug.Log($"MinMax: {minMax}, Screen: {screenSpace.Translation}");
                    // }

                    UpdateVertexSpan(tempImageData, minMax, spriteData, color);

                    unsafe {
                        var dst  = (Vertex*)vertices.GetUnsafePtr() + span.VertexSpan.x;
                        var size = UnsafeUtility.SizeOf<Vertex>() * span.VertexSpan.y;
                        UnsafeUtility.MemCpy(dst, tempImageData.GetUnsafePtr(), size);
                    }

                    CommandBuffer.AddComponent<RebuildMeshTag>(root);
                }
            }

            void UpdateVertexSpan(NativeArray<Vertex> vertices, float4 minMax, SpriteData data, float4 color) {
                vertices[0]  = new Vertex {
                    Color    = color,
                    Normal   = new float3(0, 0, -1),
                    Position = new float3(minMax.xy, 0),
                    UV1      = data.OuterUV.xy,
                    UV2      = new float2(1)
                };

                vertices[1]  = new Vertex {
                    Color    = color,
                    Normal   = new float3(0, 0, -1),
                    Position = new float3(minMax.xw, 0),
                    UV1      = data.OuterUV.xw,
                    UV2      = new float2(1)
                };

                vertices[2]  = new Vertex {
                    Color    = color,
                    Normal   = new float3(0, 0, -1),
                    Position = new float3(minMax.zw, 0),
                    UV1      = data.OuterUV.zw,
                    UV2      = new float2(1)
                };

                vertices[3]  = new Vertex {
                    Color    = color,
                    Normal   = new float3(0, 0, -1),
                    Position = new float3(minMax.zy, 0),
                    UV1      = data.OuterUV.zy,
                    UV2      = new float2(1)
                };
            }
        }

        private EntityQuery canvasQuery;
        private EntityQuery imageQuery;
        private EntityQuery textQuery;
        private EntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { 
                    ComponentType.ReadOnly<ReferenceResolution>(), ComponentType.ReadOnly<Child>(),
                    ComponentType.ReadOnly<OnResolutionChangeTag>()
                }
            });

            imageQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<SpriteData>() }
            });

            textQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<CharElement>() }
            });

            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireForUpdate(canvasQuery);
        }

        // TODO: Put this on separate threads.
        protected override void OnUpdate() {
            var charBuffers = GetBufferFromEntity<CharElement>();
            var spriteData  = GetComponentDataFromEntity<SpriteData>(true);
            var children    = GetBufferFromEntity<Child>(true);

            var images = new NativeList<Entity>(
                imageQuery.CalculateEntityCountWithoutFiltering(), 
                Allocator.TempJob);

            var stretched = new NativeList<Entity>(
                imageQuery.CalculateChunkCountWithoutFiltering(),
                Allocator.TempJob);

            var texts  = new NativeList<Entity>(
                textQuery.CalculateChunkCountWithoutFiltering(), 
                Allocator.TempJob);

            var dimensions    = GetComponentDataFromEntity<Dimension>(true);
            var colors        = GetComponentDataFromEntity<AppliedColor>(true);
            var spans         = GetComponentDataFromEntity<MeshDataSpan>(true);
            var screenSpaces  = GetComponentDataFromEntity<ScreenSpace>(true);
            var resolutions   = GetComponentDataFromEntity<DefaultSpriteResolution>(true);
            var stretch       = GetComponentDataFromEntity<Stretch>(true);
            var root          = GetComponentDataFromEntity<RootCanvasReference>(true);
            var vertexBuffers = GetBufferFromEntity<Vertex>(false);

            var collectJob           = new CollectEntitiesJob {
                CharBuffers          = charBuffers,
                Children             = children,
                Stretched            = stretch,
                EntityType           = GetEntityTypeHandle(),
                SpriteData           = spriteData,
                SimpleImgEntities    = images,
                StretchedImgEntities = stretched,
                TextEntities         = texts
            };

            collectJob.Run(canvasQuery);

            UnityEngine.Debug.Log($"Building Screen Size: {UnityEngine.Screen.width} {UnityEngine.Screen.height}, {images.Length}");

            var imageJob          = new BuildSimpleImgVertexDataJob {
                Root              = root,
                Colors            = colors,
                Dimensions        = dimensions,
                MeshDataSpans     = spans,
                ScreenSpaces      = screenSpaces,
                SpriteData        = spriteData,
                SpriteResolutions = resolutions,
                VertexData        = vertexBuffers,
                Images            = images,
                CommandBuffer     = commandBufferSystem.CreateCommandBuffer()
            };

            imageJob.Run();

            // Dispose all the temp containers
            // --------------------------
            images.Dispose();
            stretched.Dispose();
            texts.Dispose();

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
