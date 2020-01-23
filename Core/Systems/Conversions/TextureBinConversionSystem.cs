using Unity.Entities;

namespace UGUIDots {

    // TODO: Avoid using Resources - not currently the best way
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class TextureBinDeclarationSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            if (TextureBin.TryLoadTextureBin("TextureBin", out TextureBin textureBin)) {
                DeclareReferencedAsset(textureBin);
            }
        }
    }

    public class TextureBinConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            if (TextureBin.TryLoadTextureBin("TextureBin", out TextureBin textureBin)) {
                var binEntity = GetPrimaryEntity(textureBin);

                if (!DstEntityManager.HasComponent<TextureBin>(binEntity)) {
                    DstEntityManager.AddComponentObject(binEntity, textureBin);
                }
            }
        }
    }
}
