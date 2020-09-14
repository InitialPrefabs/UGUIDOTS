using Unity.Entities;

namespace UGUIDOTS.Render {

    /// <summary>
    /// Indicates that UI Element needs to be rebuilt.
    /// </summary>
    public struct BuildUIElementTag : IComponentData { }

    /// <summary>
    /// Marks that only the color needs to be updated in the mesh
    /// </summary>
    public struct UpdateVertexColorTag : IComponentData { }

    /// <summary>
    /// Marks that an element is not rendering.
    /// </summary>
    public struct DisableRenderingTag : IComponentData { }

    /// <summary>
    /// Marks that the element should render.
    /// </summary>
    public struct EnableRenderingTag : IComponentData { }
}
