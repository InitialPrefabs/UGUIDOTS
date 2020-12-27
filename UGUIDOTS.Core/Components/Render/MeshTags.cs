using Unity.Entities;

namespace UGUIDOTS.Render {

    /// <summary>
    /// Marks that the element needs to update the vertex buffer slice. This should not be confused 
    /// with rebuild which would cause all of the entities to be rebatched.
    /// </summary>
    public struct UpdateSliceTag : IComponentData { }

    public struct RebuildMeshTag : IComponentData { }
}
