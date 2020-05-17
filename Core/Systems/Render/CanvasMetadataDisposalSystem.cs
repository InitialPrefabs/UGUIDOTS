using Unity.Entities;

namespace UGUIDots.Render.Systems {

    public class CanvasMetadataDisposalSystem : SystemBase {
        protected override void OnUpdate() {

            var deps = Dependency;
            Entities.WithNone<RootVertexData>().ForEach((ref ChildrenActiveMetadata c0) => {
                c0.Dispose(deps);   
            }).ScheduleParallel(Dependency);
        }
    }
}
