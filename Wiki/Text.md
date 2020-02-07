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
The `FontAssetDeclarationSystem` declares that the TMP FontAsset's ScriptableObject
dependency that all `TextMeshProUGUI` components needs to be represented as an entity.

## FontAssetConversionSystem
This system, grabs all of the embedded FontAssets from `TextMeshProUGUI` and adds the
following components to the FontAsset entity.

| Component | Description |
|:---------:|:-----------:|
| FontID    | Stores the unique instance ID for font asset - this is used for look up |
| GlyphElement | Stores the character's associated unicode, glyph metrics, raw uvs needed to display the font |
| FontFaceInfo | Stores Unity's FaceInfo data from the FontAsset |

## TMPTextConversionSystem
Grabs all `TextMeshProUGUI`s and adds the following components to the linked entity:

| Component | Description |
|:---------:|:-----------:|
| Dimensions | The rect size for displaying the text |
| TextFontID | The mapping required to link to the FontID |
| TextOptions | The font style, size, and alignment |
| Material | The shader properties/bitmap needed to render the text |
| CharElement | Stores all characters of a string |
| MeshVertexData | Vertex information needed to make the mesh |
| TriangleIndexElement | Mesh index information needed to store |

### Building the Actual Mesh
All letters of a mesh are built to the same vertex buffer, this batches all potential meshes such that there is only 1 issued draw calls. Generally, each letter's glyph is
retrieved from the FontAsset entity, and the glyph metrics are applied to build the
quad that will display. This takes into account font scaling so that larger point
sizes match the editor time representation.

For a more detailed outlook of building text individually and rendering, take a look
at the [OpenGL tutorial](https://learnopengl.com/In-Practice/Text-Rendering), which
affects the calculation to explains it quite nicely.
