using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDOTS.Transforms {

    public static class HierarchyUtils {
        
        /// <summary>
        /// Returns the root entity based on the current child entity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetRoot(Entity child, ComponentDataFromEntity<Parent> parents) {
            if (!parents.HasComponent(child)) {
                return child;
            }

            return GetRoot(parents[child].Value, parents);
        }
    }
}
