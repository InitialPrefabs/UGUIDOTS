using UGUIDOTS.Transforms;
using UGUIDOTS.Transforms.Systems;
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
    [UpdateAfter(typeof(AnchorSystem))]
    public class BuildRenderHierarchySystem : SystemBase {

        struct Pair {
            public Entity Child;
            public Entity Root;
        }

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

            // Text
            // -----------------------------------------
            [ReadOnly]
            public BufferFromEntity<CharElement> CharBuffers;

            // TODO: When parallelizing this, best to have per thread containers.
            [WriteOnly]
            public NativeList<Pair> ImageEntities;

            [WriteOnly]
            public NativeList<Pair> TextEntities;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(EntityType);

                for (int i = 0; i < chunk.Count; i++) {
                    var entity = entities[i];
                    var children = Children[entity].AsNativeArray().AsReadOnly();
                }
            }

            void RecurseChildrenDetermineType(NativeArray<Child>.ReadOnly children, Entity root) {
                for (int i = 0; i < children.Length; i++) {
                    var entity = children[i];
                    if (SpriteData.HasComponent(entity)) {
                        ImageEntities.Add(new Pair {
                            Child = entity,
                            Root  = root
                        });
                    }

                    if (CharBuffers.HasComponent(entity)) {
                        TextEntities.Add(new Pair {
                            Child = entity,
                            Root  = root
                        });
                    }

                    if (Children.HasComponent(entity)) {
                        var grandChildren = Children[entity].AsNativeArray().AsReadOnly();
                        RecurseChildrenDetermineType(grandChildren, root);
                    }
                }
            }
        }

        // NOTE: Assume all static
        struct BuildImageVertexData : IJob {

            public EntityCommandBuffer CommandBuffer;

            public BufferFromEntity<Vertex> VertexData;

            [ReadOnly]
            public NativeList<Pair> Images;

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

            public void Execute() {
                var tempImageData = new NativeArray<Vertex>(4, Allocator.Temp);

                for (int i = 0; i < Images.Length; i++) {
                    var pair = Images[i];

                    // Get the root data
                    var vertices      = VertexData[pair.Root];
                    var rootTransform = ScreenSpaces[pair.Root];

                    // Build the image data
                    var spriteData = SpriteData[pair.Child];
                    var resolution = SpriteResolutions[pair.Child];
                    var dimension  = Dimensions[pair.Child];
                    var m          = ScreenSpaces[pair.Child].AsMatrix();
                    var color      = Colors[pair.Child].Value.ToNormalizedFloat4();

                    var minMax = ImageUtils.CreateImagePositionData(
                        resolution, spriteData, dimension, m, rootTransform.Scale.x);

                    var span = MeshDataSpans[pair.Child];

                    UpdateVertexSpan(tempImageData, pair.Child, minMax, spriteData, color);

                    unsafe {
                        var dst  = (Vertex*)vertices.GetUnsafePtr() + span.VertexSpan.x;
                        var size = UnsafeUtility.SizeOf<Vertex>() * span.VertexSpan.y;
                        UnsafeUtility.MemCpy(dst, tempImageData.GetUnsafePtr(), size);
                    }

                    CommandBuffer.AddComponent<RebuildMeshTag>(pair.Root);
                }
            }

            void UpdateVertexSpan(NativeArray<Vertex> vertices, Entity image, float4 minMax, SpriteData data, float4 color) {
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

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {  ComponentType.ReadOnly<ReferenceResolution>(), ComponentType.ReadOnly<Child>() }
            });

            imageQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<SpriteData>() }
            });

            textQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] { ComponentType.ReadOnly<CharElement>() }
            });
        }

        // TODO: Put this on separate threads.
        protected override void OnUpdate() {
            var charBuffers = GetBufferFromEntity<CharElement>();
            var spriteData  = GetComponentDataFromEntity<SpriteData>(true);
            var children    = GetBufferFromEntity<Child>(true);

            var images = new NativeList<Pair>(
                imageQuery.CalculateEntityCountWithoutFiltering(), 
                Allocator.TempJob);

            var texts  = new NativeList<Pair>(
                textQuery.CalculateChunkCountWithoutFiltering(), 
                Allocator.TempJob);

            var collectJob    = new CollectEntitiesJob {
                CharBuffers   = charBuffers,
                Children      = children,
                EntityType    = GetEntityTypeHandle(),
                SpriteData    = spriteData,
                ImageEntities = images,
                TextEntities  = texts
            };

            collectJob.Run(canvasQuery);

            var dimensions    = GetComponentDataFromEntity<Dimension>(true);
            var colors        = GetComponentDataFromEntity<AppliedColor>(true);
            var spans         = GetComponentDataFromEntity<MeshDataSpan>(true);
            var screenSpaces  = GetComponentDataFromEntity<ScreenSpace>(true);
            var resolutions   = GetComponentDataFromEntity<DefaultSpriteResolution>(true);
            var vertexBuffers = GetBufferFromEntity<Vertex>(false);

            var imageJob          = new BuildImageVertexData {
                Colors            = colors,
                Dimensions        = dimensions,
                MeshDataSpans     = spans,
                ScreenSpaces      = screenSpaces,
                SpriteData        = spriteData,
                SpriteResolutions = resolutions,
                VertexData        = vertexBuffers,
                Images            = images
            };

            imageJob.Run();

            images.Dispose();
            texts.Dispose();
        }
    }
}
