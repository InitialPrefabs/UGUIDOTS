using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render {

    public struct DynamicRenderTag : IComponentData { }

    public struct StaticRenderTag : IComponentData { }

    public class DynamicRenderTagProxy : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new DynamicRenderTag { });
        }
    }
}
