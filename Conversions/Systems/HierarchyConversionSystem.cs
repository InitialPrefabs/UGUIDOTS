using System.Collections.Generic;
using UGUIDOTS.Conversions.Analyzers;
using UGUIDOTS.Render;
using UGUIDOTS.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

namespace UGUIDOTS.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class HierarchyConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {
                if (canvas.transform.parent != null) {
                    Debug.LogError("Cannot convert a canvas that is not a root canvas!");
                    return;
                }

                var canvasEntity = GetPrimaryEntity(canvas);
                var batches      = BatchAnalysis.BuildStaticBatch(canvas);

                Debug.Log($"---------START: {canvas.name} ---------");

                foreach (var batch in batches) {
                    Debug.Log(batch[0].GameObject);
                }

                Debug.Log($"---------END: {canvas.name} ---------");

                // CanvasConversionUtils.CleanCanvas(canvasEntity, DstEntityManager);
                // CanvasConversionUtils.SetScaleMode(canvasEntity, canvas, DstEntityManager);
                BakeRenderElements(canvasEntity, batches, out var keys);
                ConstructMaterialPropertyBatchMessage(canvas, canvasEntity, keys);
                BakeVertexDataToRoot(canvasEntity, batches, out var submeshSlices);

                // Build the actual mesh needed to render.
                BuildMesh(canvasEntity, submeshSlices);
            });
        }

        private void BuildMesh(Entity canvasEntity, NativeList<SubmeshSliceElement> submeshSlices) {
            var mesh = new Mesh();

            var vertices = DstEntityManager.GetBuffer<RootVertexData>(canvasEntity).AsNativeArray();
            var indices  = DstEntityManager.GetBuffer<RootTriangleIndexElement>(canvasEntity).AsNativeArray();

            mesh.subMeshCount = submeshSlices.Length;
            mesh.SetVertexBufferParams(vertices.Length, MeshVertexDataExtensions.VertexDescriptors);
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16); 
            mesh.SetIndexBufferData(indices, 0, 0, indices.Length);

            for (int i = 0; i < submeshSlices.Length; i++) {
                var slice = submeshSlices[i];
                mesh.SetSubMesh(i, new SubMeshDescriptor {
                    bounds      = default,
                    indexStart  = slice.IndexSpan.x,
                    indexCount  = slice.IndexSpan.y,
                    firstVertex = slice.VertexSpan.x,
                    vertexCount = slice.VertexSpan.y,
                    topology    = MeshTopology.Triangles
                });
            }
            DstEntityManager.AddComponentData(canvasEntity, new SharedMesh { Value = mesh });
        }

        private unsafe void BakeVertexDataToRoot(Entity canvasEntity, List<List<BatchAnalysis.RenderedElement>> batches, 
            out NativeList<SubmeshSliceElement> submeshSlices) {
            var vertexData = new NativeList<RootVertexData>(Allocator.Temp);
            var indexData  = new NativeList<RootTriangleIndexElement>(Allocator.Temp);
            submeshSlices  = new NativeList<SubmeshSliceElement>(Allocator.Temp);

            foreach (var batch in batches) {
                var startVertex = vertexData.Length;
                var startIndex  = indexData.Length;
                foreach (var renderedElement in batch) {
                    var gameObject = renderedElement.GameObject;
                    if (gameObject.TryGetComponent(out Image image)) {
                        var indexOffset  = indexData.Length;
                        var vertexOffset = vertexData.Length;

                        var entity = GetPrimaryEntity(gameObject);
                        var m      = DstEntityManager.GetComponentData<ScreenSpace>(entity).AsMatrix();

                        var spriteData = DstEntityManager.GetComponentData<SpriteData>(entity);
                        var resolution = DstEntityManager.GetComponentData<DefaultSpriteResolution>(entity);
                        var dim        = DstEntityManager.GetComponentData<Dimension>(entity);
                        var color      = DstEntityManager.GetComponentData<AppliedColor>(entity);

                        var minMax = ImageUtils.BuildImageVertexData(resolution, spriteData, dim, m);
                        
                        // Add 4 vertices for simple images
                        // TODO: Support 9 slicing images - which will generate 16 vertices
                        vertexData.AddImageVertices(minMax, spriteData, color.Value);

                        // After each image, the index needs to increment
                        indexData.AddImageIndices(in vertexData);

                        var indexSize = indexData.Length - indexOffset;
                        var vertexSize = vertexData.Length - vertexOffset;

                        DstEntityManager.AddComponentData(entity, new MeshDataSpan {
                            IndexSpan  = new int2(indexOffset, indexSize),
                            VertexSpan = new int2(vertexOffset, vertexSize)
                        });
                    }

                    if (gameObject.TryGetComponent(out TextMeshProUGUI text)) {
                        var indexOffset  = indexData.Length;
                        var vertexOffset = vertexData.Length;

                        ConversionUtility.ConvertText(this, canvasEntity, text, ref vertexData, ref indexData);

                        var indexSize = indexData.Length - indexOffset;
                        var vertexSize = vertexData.Length - vertexOffset;

                        var textEntity = GetPrimaryEntity(text);
                        DstEntityManager.AddComponentData(textEntity, new MeshDataSpan {
                            VertexSpan = new int2(vertexOffset, vertexSize),
                            IndexSpan =  new int2(indexOffset, indexSize)
                        });
                    }
                }

                var submeshSlice = new SubmeshSliceElement {
                    IndexSpan  = new int2(startIndex, indexData.Length - startIndex),
                    VertexSpan = new int2(startVertex, vertexData.Length - startVertex)
                };

                submeshSlices.Add(submeshSlice);
            }

            var vertexBuffer = DstEntityManager.AddBuffer<RootVertexData>(canvasEntity);
            vertexBuffer.ResizeUninitialized(vertexData.Length);

            UnsafeUtility.MemCpy(
                vertexBuffer.GetUnsafePtr(), 
                vertexData.GetUnsafePtr(), 
                UnsafeUtility.SizeOf<RootVertexData>() * vertexData.Length);

            var indexBuffer = DstEntityManager.AddBuffer<RootTriangleIndexElement>(canvasEntity);
            indexBuffer.ResizeUninitialized(indexData.Length);

            UnsafeUtility.MemCpy(
                indexBuffer.GetUnsafePtr(), 
                indexData.GetUnsafePtr(), 
                UnsafeUtility.SizeOf<RootTriangleIndexElement>() * indexData.Length);
        }

        private unsafe void BakeRenderElements(Entity canvasEntity, List<List<BatchAnalysis.RenderedElement>> batches, 
            out NativeList<SubmeshKeyElement> keys) {

            keys               = new NativeList<SubmeshKeyElement>(Allocator.Temp);
            var renderEntities = new NativeList<RenderElement>(Allocator.Temp);
            var batchSpans     = new NativeList<BatchedSpanElement>(Allocator.Temp);
            int startIdx       = 0;

            // Build a flat array of of the elements we need to render and the spans which defines
            // which sections of the RenderElements belong to which batch.
            // TODO: Write more documents on this...
            for (int i = 0; i < batches.Count; i++) {
                var currentBatch = batches[i];

                for (int k = 0; k < currentBatch.Count; k++) {
                    var uiElement = GetPrimaryEntity(currentBatch[k].GameObject);
                    renderEntities.Add(new RenderElement { Value = uiElement });
                }

                var first = GetPrimaryEntity(currentBatch[0].GameObject);
                var key   = new SubmeshKeyElement {};
                if (DstEntityManager.HasComponent<LinkedTextureEntity>(first)) {
                    key.TextureEntity = DstEntityManager.GetComponentData<LinkedTextureEntity>(first).Value;
                }

                if (DstEntityManager.HasComponent<LinkedMaterialEntity>(first)) {
                    key.MaterialEntity = DstEntityManager.GetComponentData<LinkedMaterialEntity>(first).Value;
                }

                keys.Add(key);

                batchSpans.Add(new int2(startIdx, currentBatch.Count));
                startIdx += currentBatch.Count;
            }

#region Buffer Setup
            var renderBatches = DstEntityManager.AddBuffer<RenderElement>(canvasEntity);
            var size = UnsafeUtility.SizeOf<RenderElement>() * renderEntities.Length;

            renderBatches.ResizeUninitialized(renderEntities.Length);
            UnsafeUtility.MemCpy(renderBatches.GetUnsafePtr(), renderEntities.GetUnsafePtr(), size);

            var renderSpans = DstEntityManager.AddBuffer<BatchedSpanElement>(canvasEntity);
            size = UnsafeUtility.SizeOf<BatchedSpanElement>() * batchSpans.Length;
            
            renderSpans.ResizeUninitialized(batchSpans.Length);
            UnsafeUtility.MemCpy(renderSpans.GetUnsafePtr(), batchSpans.GetUnsafePtr(), size);

            var submeshKeys = DstEntityManager.AddBuffer<SubmeshKeyElement>(canvasEntity);
            size = UnsafeUtility.SizeOf<SubmeshKeyElement>() * keys.Length;

            submeshKeys.ResizeUninitialized(keys.Length);
            UnsafeUtility.MemCpy(submeshKeys.GetUnsafePtr(), keys.GetUnsafePtr(), size);
#endregion
        }
        
        // TODO: Frame one will need to create the material property batch, for now store the 
        private unsafe void ConstructMaterialPropertyBatchMessage(Canvas canvas, Entity canvasEntity, NativeList<SubmeshKeyElement> keys) {
            var msg = CreateAdditionalEntity(canvas);

#if UNITY_EDITOR
            var name = DstEntityManager.GetName(canvasEntity);
            DstEntityManager.SetName(msg, $"[Material Property Batch]: {name}");
#endif
            var buffer =  DstEntityManager.AddBuffer<SubmeshKeyElement>(msg);
            buffer.ResizeUninitialized(keys.Length);
            UnsafeUtility.MemCpy(buffer.GetUnsafePtr(), keys.GetUnsafePtr(), UnsafeUtility.SizeOf<SubmeshKeyElement>() * keys.Length);

            // Add the entity the material property will be 'assigned' to
            DstEntityManager.AddComponentData(msg, new MaterialPropertyEntity { Count = keys.Length, Canvas = canvasEntity });
        }
    }
}
