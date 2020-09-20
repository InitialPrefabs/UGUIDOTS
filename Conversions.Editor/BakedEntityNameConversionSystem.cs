using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UGUIDOTS.Conversions.Systems;

namespace UGUIDOTS.Conversions.Editor {

    internal struct BakedEntityName : IComponentData {
        internal FixedString512 Value;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    class SetEntityNameSystem : SystemBase {

        EntityQuery nameQuery;

        protected override void OnCreate() {
            nameQuery = GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] { ComponentType.ReadOnly<BakedEntityName>() }
            });

            RequireForUpdate(nameQuery);
        }

        protected override void OnUpdate() {
            var entities = nameQuery.ToEntityArray(Allocator.TempJob);
            var names = nameQuery.ToComponentDataArray<BakedEntityName>(Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++) {
                EntityManager.SetName(entities[i], names[i].Value.ToString());
            }

            entities.Dispose();
            names.Dispose();

            EntityManager.RemoveComponent<BakedEntityName>(nameQuery);
        }
    }

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateAfter(typeof(HierarchyConversionSystem))]
    class BakedEntityNameConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((RectTransform transform) => {
                var name = transform.name;

                var entity = GetPrimaryEntity(transform);
                DstEntityManager.AddComponentData(entity, new BakedEntityName { Value = transform.name });
            });

            Entities.ForEach((Image image) => {
                var mat = image.material;
                var matEntity = GetPrimaryEntity(mat);
                DstEntityManager.AddComponentData(matEntity, new BakedEntityName { Value = $"[Material]: {mat.name}" });

                var texture = image.sprite != null ? image.sprite.texture : Texture2D.whiteTexture;
                var textureEntity = GetPrimaryEntity(texture);

                DstEntityManager.AddComponentData(textureEntity, new BakedEntityName { Value = $"[Texture]: {texture.name}" });
            });

            Entities.ForEach((TextMeshProUGUI text) => {
                var mat = text.material;
                var matEntity = GetPrimaryEntity(mat);
                DstEntityManager.AddComponentData(matEntity, new BakedEntityName { Value = $"[Material]: {mat.name}" });

                var fontEntity = GetPrimaryEntity(text.font);
                DstEntityManager.AddComponentData(fontEntity, new BakedEntityName { Value = $"[Font Asset]: {text.font.name}" });
            });
        }
    }
}