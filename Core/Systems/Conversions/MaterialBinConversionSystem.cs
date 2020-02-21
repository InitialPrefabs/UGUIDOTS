using UGUIDots.Collections.Runtime;
using Unity.Entities;

namespace UGUIDots.Conversions {

    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class MaterialBinDeclarationSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            if (MaterialBin.TryLoadBin("MaterialBin", out var materialBin)) {
                DeclareReferencedAsset(materialBin);
            }
        }
    }

    public class MaterialBinConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            if (MaterialBin.TryLoadBin("MaterialBin", out var materialBin)) {
                var entity = GetPrimaryEntity(materialBin);

                if (!DstEntityManager.HasComponent<MaterialBin>(entity)) {
                    DstEntityManager.AddComponentObject(entity, materialBin);
                }
            }
        }
    }
}
