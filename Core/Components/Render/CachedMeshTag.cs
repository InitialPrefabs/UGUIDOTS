using Unity.Entities;

namespace UGUIDots.Render {

    // TODO: Add a reactive system such that if the dimensions change, then we rebuild the mesh by removing the tag.
    /// <summary>
    /// Indicates that UI Element needs to be rebuilt.
    /// </summary>
    public struct BuildUIElementTag : IComponentData { }

    // TODO: Rename the file
    public struct UpdateVertexColorTag : IComponentData { }
}
