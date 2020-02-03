using System;
using System.Collections.Generic;
using Unity.Entities;

namespace UGUIDots.Render.Systems {

    [UpdateInGroup(typeof(MeshBatchingGroup))]
    [UpdateAfter(typeof(RenderRecurseOrderSystem))]
    public class RenderSortSystem : ComponentSystem {

        public struct RenderPair : IComparer<RenderPair>, IEquatable<RenderPair> {
            public Entity Root;
            public int ID;

            public int Compare(RenderPair x, RenderPair y) {
                return x.ID.CompareTo(y.ID);
            }

            public bool Equals(RenderPair other) {
                return other.Root == Root && other.ID == ID;
            }
            public override int GetHashCode() {
                return Root.GetHashCode() ^ ID.GetHashCode();
            }
        }

        private class RenderGroupComparer : IComparer<RenderPair> {
            public int Compare(RenderPair x, RenderPair y) {
                return x.ID.CompareTo(y.ID);
            }
        }

        public List<RenderPair> SortedOrderPairs { get; private set; }

        private RenderGroupComparer comparer;
        private EntityQuery unsortedQuery;

        protected override void OnCreate() {
            comparer = new RenderGroupComparer();

            unsortedQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<UnsortedRenderTag>(),
                    ComponentType.ReadOnly<RenderGroupID>(),
                    ComponentType.ReadOnly<RenderElement>()
                }
            });

            SortedOrderPairs = new List<RenderPair>();
        }

        protected override void OnUpdate() {
            Entities.With(unsortedQuery).ForEach((Entity entity, ref RenderGroupID c0) => {
                SortedOrderPairs.Add(new RenderPair {
                    Root = entity,
                    ID   = c0.Value
                });

                PostUpdateCommands.RemoveComponent<UnsortedRenderTag>(entity);
            });

            SortedOrderPairs.Sort(comparer);
        }
    }
}
