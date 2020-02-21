using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render.Authoring {

    public class RenderCommandAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public OrthographicRenderFeature RenderFeature;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddSharedComponentData(entity, new RenderCommand {
                RenderFeature = RenderFeature
            });
        }
    }
}