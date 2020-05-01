using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;

namespace UGUIDots {

    public abstract class TestFixture {

        public class EntityForEachSystem : ComponentSystem {

            public new EntityQueryBuilder Entities => base.Entities;

            protected override void OnUpdate() { }
        }

        protected EntityForEachSystem System {
            get {
                return World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityForEachSystem>();
            }
        }

        protected EntityQueryBuilder Entities {
            get {
                return System.Entities;
            }
        }

        protected World previousWorld;
        protected World world;
        protected EntityManager manager;
        protected EntityManager.EntityManagerDebug managerDebug;

        [SetUp]
        public virtual void SetUp() {
            previousWorld = World.DefaultGameObjectInjectionWorld;
            world = World.DefaultGameObjectInjectionWorld = new World("Test World");

            manager = world.EntityManager;
            managerDebug = new EntityManager.EntityManagerDebug(manager);
        }

        [TearDown]
        public virtual void TearDown() {
            if (manager != default && manager.IsCreated) {
                while (world.Systems.Count > 0) {
                    world.DestroySystem(world.Systems[0]);
                }

                managerDebug.CheckInternalConsistency();

                world.Dispose();
                world = null;

                World.DefaultGameObjectInjectionWorld = previousWorld;
                previousWorld = null;
                manager = default;
            }
        }
    }
}