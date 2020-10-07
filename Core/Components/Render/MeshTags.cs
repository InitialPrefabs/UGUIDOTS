using System;
using Unity.Entities;

namespace UGUIDOTS.Render {

    /// <summary>
    /// Indicates that UI Element needs to be rebuilt.
    /// </summary>
    [Obsolete]
    public struct BuildUIElementTag : IComponentData { }

    /// <summary>
    /// Marks that only the color needs to be updated in the mesh
    /// </summary>
    [Obsolete]
    public struct UpdateVertexColorTag : IComponentData { }

    /// <summary>
    /// Marks that an element is not rendering.
    /// </summary>
    public struct DisableRenderingTag : IComponentData { }

    /// <summary>
    /// Marks that the element should render.
    /// </summary>
    public struct EnableRenderingTag : IComponentData { }

    /// <summary>
    /// Marks that the element needs to update the vertex buffer slice. This should not be confused 
    /// with rebuild which would cause all of the entities to be rebatched.
    /// </summary>
    public struct UpdateSliceTag : IComponentData { }

    public struct RebuildMeshTag : IComponentData { }
}
