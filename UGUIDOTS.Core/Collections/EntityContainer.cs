using Unity.Entities;

namespace UGUIDOTS.Collections {

    /// <summary>
    /// Stores an Entity in a struct, such that the element can be stored in Unsafe data structures.
    /// </summary>
    internal struct EntityContainer : IStruct<EntityContainer> {
        internal Entity Value;

        public static implicit operator EntityContainer(Entity value) => new EntityContainer { Value = value };
        public static implicit operator Entity(EntityContainer value) => value.Value;
    }

    /// <summary>
    /// Stores the priority based on the submesh index.
    /// </summary>
    internal struct EntityPriority : IStruct<EntityPriority>, IPrioritize<EntityPriority> {

        internal Entity Entity;
        internal int SubmeshIndex;

        public int Priority() {
            return SubmeshIndex;
        }
    }
}
