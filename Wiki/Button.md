# Button

Buttons are supported but in a rather verbose way. They are currently experimental and will need work further down the 
line to make it easier to support buttons.

## Setup
To setup a Button, it is highly recommended to use the TextMeshPro button if you want to have text with your button. To 
mark a button, add the following to the GameObject:

|Component | Description |
|:---------|:------------|
| ButtonTypeAuthoring | Stores the type of click button the button is |
| Button Behavior Script | A custom authored script which will tell what the button will do |
| ButtonFrameMessagePayload | A custom entity  to produce when clicked |

## Workflow
> Again this is experimental and will likely change over time.

### ButtonTypeAuthoring
The `ButtonTypeAuthoring` component defines the kind of button. 

There are currently three kinds of buttons
* ReleaseUp
* PressDown
* Held

Clicks are registered based on its type. A button type which is `ReleaseUp` will mean that a click is registered when 
the mouse is let go. This is similar to current button behaviors found in desktop operating systems (Window, macOS, \*nix). 
A button type of `PressDown` means that the moment the mouse is pressed - the button will respond. This is recommended 
if you have a something that is very time sensitive and need to have a click registered (e.g. mobile UI that requires 
rapid tapping). A button type of `Held` means that as long as the cursor is on top of the button then, a click is 
registered.

### Button Behavior Script
The Button Behavior Script is your own authored behavior that you attach to the Button GameObject. Because UGUIDots does 
not have any kind of its own native event system, it uses a Producer/Consumer model.

The custom button behavior _must_ define an entity to produce during the click. This produced entity is processed on the 
following frame and consumed on the next frame in the `PresentationCommandBufferSystem`.

For example, this is how the `ButtonSample` in the demo works:

```
// ButtonSample.cs

public struct SampleID : IComponentData {
    public int Value;
}

public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    var msgEntity = dstManager.CreateEntity();
    dstManager.AddComponentData(msgEntity, new SampleID { Value = SOME_VALUE });
    dstManager.AddComponentData(entity, new ButtonMessageFramePayload { Value = msgEntity });
}
```

The `SampleID` struct is a custom component with some data we want to produce on a click. It can be any integer we define 
in the inspector of the script. 

In the `Convert` function, we produce a dummy entity called, `msgEntity`. The `msgEntity` is added with the `SampleID` 
component data.

***Lastly***, the `msgEntity` is stored in a `ButtonMessageFramePayload` of the Button entity. This allows the `ButtonMessageProducerSystem` 
to read the linked entity of the Button and create a copy of it to signal other systems to run their update behavior. 

### Message Groups
The pipeline of how buttons work is as followed:

* Click on button
* Produce a messaging entity
* Process messaging entity to do some action (see below)
* Consume messaging entity

To support this pipeline, there are ComponentSystemGroups provided to implement your system in to process the message.

```
MessagingUpdateGroup <- where systems will update in to process the created messaging entity
MessagingConsumptionGroup <- where the ButtonMessageConsumerSystem will destroy the created messaging entity
MessagingProductionGroup <- where the ButtonMessageProducerSystem will create the message messaging entity
```
