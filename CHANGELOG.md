# Change Log

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
