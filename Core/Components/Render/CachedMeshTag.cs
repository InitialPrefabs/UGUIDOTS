using Unity.Entities;

namespace UGUIDots.Render {

    // TODO: Add a reactive system such that if the dimensions change, then we rebuild the mesh by removing the tag.
    /// <summary>
    /// Internal tag so that meshes that have been cached haven't been rechecked.
    /// </summary>
    public struct CachedMeshTag : IComponentData { }
}
