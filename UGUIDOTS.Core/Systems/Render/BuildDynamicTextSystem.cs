using Unity.Entities;

namespace UGUIDOTS.Render.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(BuildRenderHierarchySystem))]
    [DisableAutoCreation]
    public unsafe class BuildDynamicTextSystem : SystemBase {

        protected override void OnUpdate() {
            // TODO: Collect the entities
            // TODO: Figure out the static spans
            // TODO: Apply the mesh slice update to dynamic text.
        }
    }
}
