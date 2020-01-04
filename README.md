# UGUIDots

Current project of transforming the default UGUI into a DOTS compliant implementation.

## Why do this?
A DOTS compliant UI is still underway (which will be based off of UIElements). Until then, I need a UI solution,
that most designers can get familiar with - without building custom tooling. Similarly - one that has the performance
capabilities for both mobile and desktops. That said supported platforms are primarily for:

* Android
* iOS
* Linux 64 bit
* macOS
* Windows 64 bit

WebGL may be a hit or miss.

## TODO

Support for the following will come over time - depends on the needs for my own game.

(?) - Maybe I'll support?

* [x] Canvas Scaling
* [x] Anchoring
* [x] Image stretching
* [ ] Button actions
* [ ] Button states (with color support)
* [ ] Hierarchy based disabling
* [ ] Text Rendering
* [ ] Input fields
* [ ] Subscene caching
* [ ] Convert reference types to use pointers to avoid chunk splitting (?)
* [ ] Support GPU instancing for duplicate UI elements (?)

## QuickStart

All UGUI elements are converted into their entities format via the ConversionSystems (depends on what is currently supported).
When entering play mode, the root Canvases are destroyed and render instructions are pushed into an Orthographic Render Pass.

You will need a gameObject with the `RenderComandProxy` proxy attached. This is where the RenderPipelineFeature is stored
and needed by the `MeshRenderSystem` to enqueue GPU instructions to render the UI.

## Sample Scene

Open RectTransformConversionTest.unity to see how scenes are set up.

## Dependencies

* Unity 2019.3fx

Grab these from Unity's package manager.

* Burst 1.1.2
* Entities 0.4.0-preview-10
* Jobs 0.2.2-preview-6
* Collections 0.4.0-preview-6
* UGUI 1.0.0

## Limitations
Development is still underway - so not all of the features Unity has by default is supported. Similarly - there are certain
cases that are not accounted for, like sub canvases as I barely use features like that.
