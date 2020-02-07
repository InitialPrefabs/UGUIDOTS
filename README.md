# UGUIDots

UGUIDots is a Data Oriented Tech Stack library aimed to bridge the gap in between
[Unity's WYSIWYG UI](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/index.html) and
the [Entity Component System](https://docs.unity3d.com/Packages/com.unity.entities@0.1/manual/index.html). This is a **low level library** which augments on top of Unity's UI and does not serve as a
replacement - so the workflow of authoring UI designs in games largely remains the same.

## Why do this?
A DOTS compliant UI is still underway (which will be based off of UIElements). Until then, I need a UI solution, that most designers can get familiar with - without building too many custom tooling.
Similarly - one that has the performance capabilities for both mobile and desktops. That said
supported platforms are primarily for:

* Android
* iOS
* Linux 64 bit
* macOS
* Windows 64 bit

## Wiki
The [wiki](Wiki/Home.md) is currently being worked on and contains basic information about Image/Text pipelines.

## Changelog
General purpose change log can be found [here](CHANGELOG.md).

## TODO

Support for the following will come over time - depends on the needs for my own game.

(?) - Maybe I'll support?

* [x] Canvas Scaling
* [x] Anchoring
* [x] Image stretching
* [ ] Button actions
* [ ] Button states (with color support)
* [ ] Hierarchy based disabling
* [x] Text Rendering
* [ ] Input fields
* [ ] Subscene caching
* [ ] Convert reference types to use pointers to avoid chunk splitting (?)
* [ ] Support manual image / text batching of static UI elements (static analysis)

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

* Burst 1.2.1
* Entities 0.5.1-preview-11
* Jobs 0.2.2-preview-11
* Collections 0.5.1-preview-11
* UGUI 1.0.0

## Limitations
Development is still underway - so not all of the features Unity has by default is supported. Similarly - there are certain
cases that are not accounted for, like sub canvases as I barely use features like that.

## Credits
Some thanks to a few folks who've helped me figure out things along the way

* Valentina S.
* Arthur D.
* Mason R.
