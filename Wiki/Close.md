# Close

In UGUI, you can typically "close" a UI element by disabling a GameObject. Unity rebuilds the canvas and displays the 
new rendered elements.

To achieve this functionality in UGUIDOTS, it is implemented by extending the Button messaging system/event system.

## Background Information
All GameObjects have metadata to determine whether or not it's active. This is typically done through the `activeInHierarchy` 
flag found in all GameObject. To emulate this behavior in a similar way, each canvas stores the state of its children 
with a component called `ChildrenActiveMetadata`.

This allows us to determine whether an entity was enabled or disabled to begin with and mimic the same behaviour as 
ticking the active toggle box in each GameObject.

For example, imagine a subgroup within the UI with 3 active elements and 1 inactive element. On start, we know that the 
subgroup's children will have 3 active elements and 1 inactive element, so the `ChildrenActiveMetadata` component will 
store this into its UnsafeHashMap.

> The `ChildrenActiveMetadata` is populated on conversion via the `CanvasConversionSystem`.

## Workflow 
You would typically attach `CloseButtonAuthoring` component to the button. This component allows you to store multiple 
instances of GameObjects as targets. The `CloseButtonAuthoring` remaps the GameObjects' to their entity version and 
stores a `DynamicBuffer` of targetted entities to close, show, or toggle.

Determining the functionality is driven by the enum specified in the `CloseButtonAuthoring`. For example, a Type of 
Toggle will attach the `ToggleButtonType` component to the entity and switch the behavior.

## Runtime
When the button is clicked, the `ToggleVisibilitySystem` will loop through all the targets that intend to close/show.
The system goes through each element's child recursively and ensures that all its children are disabled. Only the target 
entities active state metadata is flipped. The children's metadata are not flipped so that the last known active states 
are consistent and when the subgroup is renabled - the correct elements can be displayed properly. Entities are 
"disabled" by attaching the `Disabled` component to the target entites and their children. They are renabled when the 
`EnableRenderingTag` is added back to the entity.

The state of the entity determines how the `UpdateLocalMeshDataSystem` to manipulate the vertices:

* If the entity is `Disabled` and does not have `EnableRenderingTag`, then the entity is newly disabled and the vertices 
are moved out of view.
* If the entity has the `EnableRenderingTag` and the `Disabled` tag is removed, then the entity is renabled and the 
vertices are moved into view.
* If the entity has neither, then the entity is currently enabled and rendering.

See the `CloseButtonSample` for a demo of how this works.
