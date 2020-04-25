# UGUIDots

<p align="center">
    <img src="Wiki/Images/uguidots-logo.png" alt="Logo done by Sabrina Lam">
</p>

UGUIDots is a Data Oriented Tech Stack library aimed to bridge the gap in between
[Unity's WYSIWYG UI](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/index.html) and
the [Entity Component System](https://docs.unity3d.com/Packages/com.unity.entities@0.1/manual/index.html). 
This is a **low level library** which augments on top of Unity's UI and does not serve as a
replacement - so the workflow of authoring UI designs in games largely remains the same.

## Why do this?
A DOTS compliant UI is still underway (which will be based off of UIElements). Until then, I need a UI solution, that 
most designers can get familiar with - without building too many custom tooling. Similarly - one that has the 
performance capabilities for both mobile and desktops. That said supported platforms are primarily for:

* Android
* iOS
* Linux 64 bit
* macOS
* Windows 64 bit

## Wiki
The [wiki](Wiki/Home.md) is currently being worked on and contains basic information about Image/Text pipelines.

## Changelog
General purpose change log can be found [here](CHANGELOG.md).

## Contributions
If you would like to help contribute to the development of UGUIDots, please see the contribution guidelines [here](CONTRIBUTING.md).

## TODO

Support for the following will come over time - depends on the needs for my own game.

(?) - Maybe I'll support?

* [x] Canvas Scaling
* [x] Anchoring
* [x] Image stretching
* [x] Button actions
* [x] Button states (with color support)
* [ ] Hierarchy based disabling
* [x] Text Rendering
* [ ] Input fields
* [ ] Subscene support
* [x] Support manual image / text batching of static UI elements (static analysis)

## Installation

### OpenUPM
There is currently a known issue with OpenUPM, where Unity internal scopes are added to the registry. After adding the 
package via OpenUPM, head over to the manifest file and delete the `com.unity.*` under the `scopes` entry.

```
cd <path-to-project>
openupm add com.initialprefabs.uguidots
```

### Git Submodule

```
git submodule add https://github.com/InitialPrefabs/UGUIDots.git <path-to-folder>
```

### Manually
Download the latest [release](https://github.com/InitialPrefabs/UGUIDots/releases) and add it to your project directly.


## QuickStart

All UGUI elements are converted into their entities format via the ConversionSystems (depends on what is currently supported).
When entering play mode, the root Canvases are destroyed and render instructions are pushed into an Orthographic Render Pass.

You will need a gameObject with the `RenderComandProxy` proxy attached. This is where the RenderPipelineFeature is stored
and needed by the `MeshRenderSystem` to enqueue GPU instructions to render the UI.

## Sample Repository

Please see the Sample repository [here](https://github.com/InitialPrefabs/UGUIDots.Samples).

## Dependencies

* Unity 2019.3fx

Grab these from Unity's package manager.

* Burst 1.3.0
* Entities 0.8.0-preview-8
* Jobs 0.2.7-preview-11
* Collections 0.7.0-preview-2
* UGUI 1.0.0

### Note
Regarding Entities 0.8.0-preview-8 there is an issue regarding the preprocessor defines for Android. Please follow the 
instructions in this [Unity forum post](https://forum.unity.com/threads/entities-0-8-compile-error-in-android-platform.846583/) 
to resolve it.

## Limitations
Development is still underway - so not all of the features Unity has by default is supported. Similarly - there are certain
cases that are not accounted for, like sub canvases as I barely use features like that.

## Credits
Some thanks to a few folks who've helped me figure out things along the way

* Sabrina Lam (Logo artwork)
* Valentina S.
* Arthur D.
* Mason R.
