using Unity.Entities;

namespace UGUIDots {

    /**
     * Update material property blocks
     * ResetMaterialGroup
     * UpdateMaterialGroup
     *
     * Update all transforms:
     * UITransformUpdateGroup
     * UITransformConsumerGroup
     * UITransformProducerGroup
     *
     * Update mesh properties
     * MeshUpdateGroup
     * MeshBuildGroup
     * MeshBatchGroup
     * MeshRenderGroup
     */

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UITransformProducerGroup))]
    public class MeshUpdateGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshUpdateGroup))]
    public class MeshBuildGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshBuildGroup))]
    public class MeshBatchGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshBatchGroup))]
    public class MeshRenderGroup : ComponentSystemGroup { }

    // Logic based group
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UpdateMaterialGroup))]
    public class UITransformUpdateGroup : ComponentSystemGroup { }

    // Consumer based group
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UITransformUpdateGroup))]
    public class UITransformConsumerGroup : ComponentSystemGroup { }

    // Producer based group
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UITransformConsumerGroup))]
    public class UITransformProducerGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ResetMaterialGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ResetMaterialGroup))]
    public class UpdateMaterialGroup : ComponentSystemGroup { }
}
