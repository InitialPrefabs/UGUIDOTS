using Unity.Entities;
using UnityEngine;

namespace UGUIDots.Render.Authoring {

    public class MaterialPropertyAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity {

        [Tooltip("What is the shader property that should be set?")]
        public string[] PropertyNames;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var buffer = dstManager.AddBuffer<Float4MaterialPropertyParam>(entity);

            foreach (var name in PropertyNames) {
                buffer.Add(new Float4MaterialPropertyParam(Shader.PropertyToID(name)));
            }
        }
    }
}