# Rendering

## Orthographic Render Pass
To support URP, a custom render pass has been built whose sole purpose is to do an orthographic projection.

The limitation of the render pass is that, if you need faster rendering times, multiple GraphicsCommandBuffers
are needed. Currently there is only _1_ and there is no API that allows multiple instances of a
GraphicsCommandBuffer.

## RenderSortSystem
To match Unity's rendering order which uses DFS, render entities are created for each
canvas which are populated by `RenderElement`s storing the children entities. The
`RenderElement`s are sorted in depth first traversal so that elements which are last are rendered first, and elements higher in the hierarchy are rendered last.

`RenderElement`s are only sorted when the Canvas is issued a `DirtyTag`, meaning
that if you need to add a new UI element, you should add the `DirtyTag` to the Canvas.

## HybridMeshRenderSystem
The `HybridMeshRenderSystem` collects these render entities and iterates through them
grabbing associated components: `Material`, `Texture Key`, `LocalToWorld`,
`MaterialPropertyBlock` display the text or image. This is where instructions are
built and pushed to the `OrthographicRenderPass`. The `OrthographicRenderPass` simply
reads and executes each instruction built on rendering and flushes out the
instruction queue on the render frame.