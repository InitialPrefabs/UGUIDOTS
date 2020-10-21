using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Core.Diagnostics {

    public class DebugRenderAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity {

#pragma warning disable 649
        [SerializeField]
        private OrthographicDebugRenderFeature renderFeature;
#pragma warning restore 649

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new DebugRenderCommand {
                Value = renderFeature
            });
        }
    }
}
