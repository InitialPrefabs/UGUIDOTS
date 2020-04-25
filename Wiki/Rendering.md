# Rendering

## Orthographic Render Pass
To support URP, a custom render pass has been built whose sole purpose is to do an orthographic projection.

The limitation of the render pass is that, if you need faster rendering times, multiple GraphicsCommandBuffers
are needed. Currently there is only _1_ and there is no API that allows multiple instances of a
GraphicsCommandBuffer.

## OrthographicMeshRenderSystem
By default - we retrieve the OrthographicRenderFeature via the `RenderCommandProxy`. We iterate through each Canvas,
retrieve the attached mesh and push the mesh to the an InstructionQueue embedded into the `OrthographicRenderPass`. The
`OrthographicRenderPass` ends up flushing the instruction queue and executes the context of the `GraphicsCommandBuffer`
in orthographic view. This happens every render frame.

## Limitations
Currently, there is no way to add multiple `GraphicsCommandBuffer` - as adding multiple command buffers would mean
parallel context execution on the render side.
