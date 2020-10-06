using Unity.Entities;

namespace UGUIDOTS {

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
    [System.Obsolete]
    public class MeshUpdateGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshUpdateGroup))]
    [System.Obsolete]
    public class MeshBuildGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshBuildGroup))]
    [System.Obsolete]
    public class MeshBatchGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(MeshBatchGroup))]
    [System.Obsolete]
    public class MeshRenderGroup : ComponentSystemGroup { }

    // Logic based group
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UpdateMaterialGroup))]
    [System.Obsolete]
    public class UITransformUpdateGroup : ComponentSystemGroup { }

    // Consumer based group
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UITransformUpdateGroup))]
    [System.Obsolete]
    public class UITransformConsumerGroup : ComponentSystemGroup { }

    // Producer based group
    [System.Obsolete]
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(UITransformConsumerGroup))]
    public class UITransformProducerGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [System.Obsolete]
    public class ResetMaterialGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ResetMaterialGroup))]
    [System.Obsolete]
    public class UpdateMaterialGroup : ComponentSystemGroup { }
}
