# Canvas

Canvases are small entities which contain a dynamic buffer of children entities. The primary role of the Canvas is to
manage the scale of the canvas such that the UI displays at a proper scale regardless of screen size.

## CanvasConversionSystem
All canvases that are either in a subscenes or have the `ConvertToEntity` trigger will have the following components
attached to them:

|Component | Description |
|:--------:|:------------|
| CanvasVertexData | Stores all of the vertex data of all Images and Text children |
| CanvasTriangleIndexElement | Stores all of the indices of all Image and Text children |
| RenderElement | Stores children entities which are part of the batch |
| BatchedSpanElement | Stores indices of submesh's vertices and indices |
| SubMeshKeyElement | Stores texture and material keys of each submesh - used to grab the actual textures and materials from the Bins |
| Child | Stores all children of the canvas in breadth first fashion |
| CanvasSortOrder | Determines the render order of the Canvas, canvases with lower priority are rendered first. |
| WidthHeightRatio | If the canvas is set to scale with screen size, then the canvas follows a logarithmic curve if the current resolution does not match the reference resolution. |
| DirtyTag | Marks that a canvas has not been processed yet (see the page about Rendering) |

## Limitations

Currently, canvases that use ***Constant Pixel Size*** and ***Constant Physical Size*** are not
supported.

## Rendering

Canvases alone are not rendered because they hold no rendering data, but its children are, which
typically are composed of _Images_ and _Text_.

When the canvas is initially converted to its entity format by default, they are marked with the
`DirtyTag`. This ensures that we build the list of all entities we need to render.