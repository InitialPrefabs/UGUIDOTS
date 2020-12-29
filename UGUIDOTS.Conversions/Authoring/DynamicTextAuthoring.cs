using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS.Render.Authoring {

    /// <summary>
    /// This interface lets the conversion system know to batch this text element last because 
    /// it is considered dynamic, so the number of characters can change.
    /// </summary>
    public interface IAuthorableText { }

    /// <summary>
    /// This is not actually an authoring component, but a tag to detect whether text 
    /// is dynamic.
    /// </summary>
    public class DynamicTextAuthoring : MonoBehaviour, IAuthorableText, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent<DynamicTextTag>(entity);
        }
    }
}
