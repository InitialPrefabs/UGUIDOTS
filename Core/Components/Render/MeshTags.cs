using Unity.Entities;

namespace UGUIDots.Render {

    /// <summary>
    /// Indicates that UI Element needs to be rebuilt.
    /// </summary>
    public struct BuildUIElementTag : IComponentData { }

    public struct UpdateVertexColorTag : IComponentData { }
}
