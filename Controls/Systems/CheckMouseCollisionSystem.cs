using Unity.Entities;
using Unity.Jobs;

namespace UGUIDots.Controls.Systems {

    // TODO: Implement whether a mouse is above an element
    [UpdateInGroup(typeof(InputGroup))]
    public class CheckMouseCollisionSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
