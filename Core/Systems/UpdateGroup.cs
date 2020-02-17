using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots {

    [UpdateInGroup(typeof(SimulationSystemGroup)), UpdateAfter(typeof(TransformSystemGroup))]
    public class UGUITransformSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UITransformUpdateGroup))]
    public class MeshBuildGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshBuildGroup))]
    public class MeshBatchGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshBatchGroup))]
    public class MeshRenderGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UITransformUpdateGroup : ComponentSystemGroup { }
}
