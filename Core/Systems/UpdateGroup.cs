using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots {

    [UpdateInGroup(typeof(SimulationSystemGroup)), UpdateAfter(typeof(TransformSystemGroup))]
    public class UGUITransformSystemGroup : ComponentSystemGroup { }
}
