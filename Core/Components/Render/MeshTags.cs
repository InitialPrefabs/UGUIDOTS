using Unity.Entities;

namespace UGUIDots.Render {

    /// <summary>
    /// Indicates that UI Element needs to be rebuilt.
    /// </summary>
    public struct BuildUIElementTag : IComponentData { }

    /// <summary>
    /// Marks that only the color needs to be updated in the mesh
    /// </summary>
    public struct UpdateVertexColorTag : IComponentData { }

    /// <summary>
    /// Marks an element to be not be interactable, an alternative to Disabled without the limitations.
    /// </summary>
    public struct NonInteractableTag : IComponentData { }

    /// <summary>
    /// Marks that an element is not rendering.
    /// </summary>
    public struct DisableRenderingTag : IComponentData { }

    /// <summary>
    /// Marks that the element should render.
    /// </summary>
    public struct EnableRenderingTag : IComponentData { }
}
