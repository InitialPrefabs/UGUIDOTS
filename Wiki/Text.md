# Text

Currently only TextMeshPro is supported, because they have a generated font atlas and it is easier to figure out
how to scale the text to match its point size. Default UGUI Text components are not supported due to its reliance
of the FontEngine and needing to sample on runtime the actual point sizes.

## Conversion Pipeline
TextMeshProUGUIs are converted in several stages - as there are external components needed to render text.

* FontAssetDelcarationSystem
* FontAssetConversionSystem
* TMPTextConverionSystem

## FontAssetDeclarationSystem
The `FontAssetDeclarationSystem` declares the FontAsset scriptable object dependency that all `TextMeshProUGUI`
components needs to be represented as an entity.

## FontAssetConversionSystem
This system, grabs all of the embedded FontAssets and adds the following components to the linked FontAsset
entity.

* FontID
 * The unique instance ID for the FontAsset, used for lookup.
* GlyphElement Buffer
 * Stores all characters, glyph data, and uvs from the FontAsset into the buffer

## TMPTextConversionSystem
Grabs all `TextMeshProUGUI`s and adds the following components to the linked entity:

* Dimensions
* AppliedColor
* TextFontID
* TextOptions
* Material
* CharElement Buffer
* MeshVertexData Buffer
* TriangleIndexElement Buffer

The `TextFontID` is the unique identifier that maps to the font asset so that generated text can read the correct
glyph elements for the correct quads.

`TextOptions` store alignment, style, and point size of each individual text component.

### Building the Actual Mesh
All letters of a mesh are built to the same vertex buffer, this batches all potential meshes such that there is
only 1 issued draw calls. Generally, each letter's glyph is retrieved from the FontAsset entity, and the glyph
metrics are applied to build the quad that will display. This takes into account font scaling so that larger 
point sizes match the editor time representation.

For a more detailed outlook of building text individually and rendering, take a look at the
[OpenGL tutorial](https://learnopengl.com/In-Practice/Text-Rendering), which explains it quite nicely.
