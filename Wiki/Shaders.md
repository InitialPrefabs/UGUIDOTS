# Shaders

UGUIDOTS provides specialized materials and that are compatible to URP to handle _Translation_ and _FillAmount_.

## Default UI Material

The `Default UI Material` works with Universal Render Pipeline, but has limitations in that the transformation matrix 
has to be updated in the mesh exclusively.

If a UI element has to move, instead of exclusively just updating the transformation matrix, we can copy the translation 
to the shader instead.

## Material Property Index

All images and text elements have a `MaterialPropertyIndex` component which allows the element to access the index 
of the `MaterialPropertyBatch` in the root canvas.

By accessing the index, you are able to retrieve the associated material property and manipulate any properties.


## DefaultImage Material
The `DefaultImage` material is very similar to the DefaultUI material, except that it allows for translation and fill.

The `UIPingPongSystem` and `HeartFillSystem` in the [UGUIDots.Samples](https://github.com/InitialPrefabs/UGUIDots.Samples) 
provides an example of updating the translation and fill property.

* For translation, simply pass a Vector4 to the Translation property. The translation is added in the Vertex pass in 
local space and the matrix is converted in world space when rendering.
* For fill, ensure that the shader supports fill amount and pass a float to the FillAmount property.

***All properties that are supported can be found in the `ShaderConstants` static class.***

## Limitations

### Canvas Rebuilds
* If you have a continuously moving element in your UI and you schedule a Canvas rebuild - original transformation is not 
properly resetted. This creates a very odd offset effect.

### Batching
Each material property is associated with a collection of elements.

Imagine a canvas with 3 batches. There is _only_ 1 material property per batch, as a batch is a collective representation 
of the same properties.

* Canvas
  * A -> Material Property A
  * B -> Material Property B
  * C -> Material Property C

If you need to update UI Elements individually that are in the same batch, currently, the only recommended way is to 
separate the elements from the batch. Currently the `HeartFillSystem` does this if you inspect the # of batches in the 
Fill Canvas in the sample.

> I understand that this is a cumbersome workflow. I will likely introduce a much more comprehensive analysis window in 
the future to handle this situation better.
