using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace UGUIDots {

    // TODO: Avoid using Resources - not currently the best way
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class TextureBinDeclarationSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            var textureBin = Resources.Load<TextureBin>("TextureBin");

            Assert.IsNotNull(textureBin, "TextureBin was not created in Assets/Resources");

            if (textureBin) {
                DeclareReferencedAsset(textureBin);
            }
        }
    }

    public class TextureBinConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            var textureBin = Resources.Load<TextureBin>("TextureBin");

            Assert.IsNotNull(textureBin, "TextureBin was not created in Assets/Resources");

            if (textureBin) {
                var binEntity = GetPrimaryEntity(textureBin);

                if (DstEntityManager.HasComponent<TextureBin>(binEntity)) {
                    DstEntityManager.AddComponentObject(binEntity, textureBin);
                }
            }
        }
    }
}
