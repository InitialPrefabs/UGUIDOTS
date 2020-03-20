# Canvas

Canvases are small entities which contain a dynamic buffer of children entities. The primary role of the Canvas is to
manage the scale of the canvas such that the UI displays at a proper scale regardless of screen size.

## CanvasConversionSystem
All canvases that are either in a subscenes or have the `ConvertToEntity` trigger will have the following components
attached to them:

|Component | Description |
|:---------|:------------|
| RootVertexData | Stores all of the vertex data of all Images and Text children |
| RootTriangleIndexElement | Stores all of the indices of all Image and Text children |
| RenderElement | Stores children entities which are part of the batch |
| BatchedSpanElement | Stores the start index of the first entity in the batch, and the number of elements in the batch |
| SubMeshKeyElement | Stores texture and material keys of each submesh - used to grab the actual textures and materials from the Bins |
| SubMeshSliceElement | Stores the start index and length of each submesh's vertices and indices, this helps describe the submesh boundaries for rendering |
| Child | Stores all children of the canvas in breadth first fashion |
| CanvasSortOrder | Determines the render order of the Canvas, canvases with lower priority are rendered first. |
| WidthHeightRatio | If the canvas is set to scale with screen size, then the canvas follows a logarithmic curve if the current resolution does not match the reference resolution. |
| BatchCanvasTag | Marks the canvas' indices and vertices as invalid and need to be adjusted (shifted/rebuilt) |
| BuildCanvasTag | Marks that the canvas' submeshes have to be rebuilt |
| Mesh | The actual mesh used to render the canvas |

## Limitations

Currently, canvases that use ***Constant Pixel Size*** and ***Constant Physical Size*** are not
supported.

## Rendering

Similar to UGUI - canvases are typically the primary renderers - meaning that all children of the Canvas potentially
have render information that should be passed to the Canvas. This means that all images and text components are
typically not renderers - but are suppliers of mesh information to the Canvas.


### Why do this?
The theory behind this is that we can store many canvases into a particular chunk - alongside the mesh data. This allows
a singular archetype to be queried and read simply iterating on all the chunks instead of jumping between archetypes to
render the UI.
