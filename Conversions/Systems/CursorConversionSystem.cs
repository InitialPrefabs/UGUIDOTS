using Unity.Transforms;

namespace UGUIDOTS.Conversions.Systems {
    internal class CursorConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((CursorAuthoring cursor) => {
                var entity = GetPrimaryEntity(cursor);
                DstEntityManager.RemoveComponent<LocalToWorld>(entity);
                DstEntityManager.RemoveComponent<Translation>(entity);
                DstEntityManager.RemoveComponent<Rotation>(entity);
                DstEntityManager.RemoveComponent<Scale>(entity);
                DstEntityManager.RemoveComponent<NonUniformScale>(entity);
            });
        }
    }
}
