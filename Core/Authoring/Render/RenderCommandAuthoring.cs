using System;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Render.Authoring {

    public class RenderCommandAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public OrthographicRenderFeature RenderFeature;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            if (ReferenceEquals(RenderFeature, null)) {
                throw new ArgumentException("The RenderFeature cannot be null!");
            }

            dstManager.AddComponentData(entity, new RenderCommand { RenderFeature = RenderFeature });
        }
    }
}
