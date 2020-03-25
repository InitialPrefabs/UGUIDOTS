# Material

Like Textures, Materials are declared their own entities by looping through all converted `Image` and `TextMeshProUGUI` 
components via the `VisualAssetDeclarationSystem`.

All material entities are linked to their associative entities via the `LinkedMaterialEntity` component.
