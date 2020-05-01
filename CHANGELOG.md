# Change Log

## 0.1.2
### Added
* Documentation on how buttons work (marked as experimental)
* Added TestFixtures to regressively add tests on all the systems
* Added Mobile unit tests to check if the button state is correct

### Fixed
* Fixed the MobileMouseCollisionSystem button logic from continuously producing a request every frame

### Removed
* `ButtonMessagePersistentPayload` in favor of producing an entity each frame

## 0.1.1 - 2020-04-26
### Added
* Adds a check to ensure that the rendered element has a SharedMaterial attached.

## 0.1.0 - 2020-04-25
### Added
* Added support for MaterialPropertyBlocks in the `RenderInstruction` struct
* Added a `DefaultImage` shader to handle image fill amounts and element translation
* Added `ResetMaterialGroup` to clear all property blocks
* Added `UpdateMaterialGroup` to have a system group that runs after the `ResetMaterialGroup` to apply custom properties
* Added a `HierarchyUtils` class for a common recursive operation to grab the root element
* Added constant Shader IDs to the `ShaderIDConstants` for Translation/Fill

### Changed
* `SharedTexture` and `SharedMaterial` are now managed `IComponentData` instead
* Removes the `_tempBlock` material property from the `OrthographicRenderPass`

## 0.0.10 - 2020-04-11
### Changed
* Split `InputDataAuthoring` into 2 modules for Mobile and Standalone.
* Fixed a bug with the `CopyTouchDataSystem` where the data wasn't copied from the touch module into the TouchElement buffer.

## 0.0.9 - 2020-04-10
### Changed
* Updated entities dependency to 0.9.0-preview.6
* Moved mobile logic to the if define preprocessor in the `InputDataAuthoring`.

## 0.0.8 - 2020-04-08
* No visible changes between v0.0.7 and v0.0.8. This is due to me forgetting to update package.json.

## 0.0.7 - 2020-04-07
### Added
* Added `AddMeshTag` which signals that an entity needs to have a mesh added to the entity
* Added `BuildUIElementTag` to signal that the vertex/index buffers need to be regenerated
* Added `VisualAssetConverionSystem` to declare and convert Textures and Entities into their own assets

### Changed
* Changes the RectTransformConversionSystem to attach relative dimensions on all elements in the hierarchy
* Changes `AnchorState` enum to be explicit with the preset corner to allow for easy debugging in the EntityDebugger
* Fixed `AnchorSystem` from targetting non renderered visual elements
* Fixed `AnchorSystem` to target the relative parent's dimension when computing a new anchor when the resolution changes
* Fixed the `CanvasScalerSystem` to scale the canvas only on a resolution change
* Removes `BuildTextTag`, `CachedMeshTag` in favor of `BuildUIElementTag`
* Removes the `TextureKey` and `MaterialKey` in favor of `LinkedTextureEntity` and `LinkedMaterialEntity`

## 0.0.6 - 2020-03-24
### Added
* `MessageUpdateGroup` was added so data can be processed before consumption

## 0.0.5 - 2020-03-20
### Added
* Static analysis batching of the canvas hierarchy
* Added systems to aggregate children mesh vertices and indices
* Added VisualAssetConversionSystem to declare prefab entities for textures and materials

### Changed
* Changed IJobForEach structs to use the Entities.Lambda option
* Removed RenderRecurseOrderSystem because the batcher builds the meshes in order
* Removed CanvasSortOrder for the time being because there currently isn't a good way to sort it currently
* Removed MeshIndex because meshes are attached to Canvas root objects instead.
* Removed SubmeshKeyElement indices
* Removed Bin<T> which stored project assets
* Removed TextureKey and MaterialKey index operations of retrieving keys.

## 0.0.4 - 2020-02-06
### Added
* Supports text wrapping for multiline texts
### Changed
* Added more documentation to the wiki about Canvases, Text, Images

## 0.0.3 - 2020-02-03
### Added
* Adds support for TextMeshPro
* Adds fontAtlas conversion system
* Adds basic vertical (top, middle, bottom) and horizontal left alignment
* Adds style checking for normal styles and **bold** fonts.
* Adds scaling for text and images
### Changed
* ***Removes support for UGUI Text components, in favour of TextMeshPro***
* Removes implicit linking of dimensions to mesh size, opted to embed entities with vertex and index data
* Fixes uv mapping issue of sprites
* Fixes image dimension padding

## 0.0.2 - 2020-01-18
### Added
* Added glyph metric support for regular text components
* Added conversion systems to the pipeline to automatically attach components to text components
### Changed
* Adds safety checks to stop the render pass from executing empty instructions.

## 0.0.1 - 2020-01-04
### Added
* Recursive hierarchy sorting to render elements
* Adds image fill for backgrounds

## 0.0.0 - 2019-12-29
### Added
* Adds basic rendering systems to display simple images
* Adds MeshUtil for easy generation of quads
* Adds a basic orthographic render pass to URP
