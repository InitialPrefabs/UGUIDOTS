using UGUIDots.Collections.Runtime;
using Unity.Entities;

namespace UGUIDots.Conversions {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class TextureBinDeclarationSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            if (TextureBin.TryLoadBin("TextureBin", out var textureBin)) {
                DeclareReferencedAsset(textureBin as TextureBin);
            }
        }
    }

    public class TextureBinConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            if (TextureBin.TryLoadBin("TextureBin", out var textureBin)) {
                var binEntity = GetPrimaryEntity(textureBin as TextureBin);

                if (!DstEntityManager.HasComponent<TextureBin>(binEntity)) {
                    DstEntityManager.AddComponentObject(binEntity, textureBin);
                }
            }
        }
    }
}
