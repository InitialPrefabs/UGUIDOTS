# Conversions

If you're not familiar with the GameObject Conversion Pipeline in Unity's ECS architecture, it is a way to convert 
Unity's GameObjects to their Entities version.

Typically there are a few ways to author conversions.

## IConvertGameObjectToEntity
An interface implemented into a MonoBehaviour. Typically you override the following function:

```
Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) { ... }
```

You can add component data and dynamic buffers within the function body.

## GameObjectConversionSystem
`GameObjectConversionSystems` are specialized `ComponentSystems` to process converted GameObjects. UGUIDOTs utilizes 
`GameObjectConversonSystem` to convert the entire Canvas hierarchy into their entity version with the correct components.

For more detailed instructons on the _entire_ conversion 5argon's blog about [conversion](https://gametorrahod.com/game-object-conversion-and-subscene/).

Currently in UGUIDOTS:

Conversion currently happen on the following UI components:

* [Image](Image.md)
* [Text (TextMeshPro UI)](Text.md)
* [Button](Button.md)

You can view each of their following links to view which components are added to the entity.
