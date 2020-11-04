using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS {
    public class ButtonDisableTypeAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        public GameObject[] Targets;

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            if (Targets.Length == 0) {
                return;
            }
            var entities = new NativeArray<Entity>(Targets.Length, Allocator.Temp);

            for (int i = 0; i < entities.Length; i++) {
                entities[i] = conversionSystem.GetPrimaryEntity(Targets[i]);
            }

            var targets = dstManager.AddBuffer<TargetEntity>(entity);
            targets.ResizeUninitialized(entities.Length);

            UnsafeUtility.MemCpy(
                targets.GetUnsafePtr(), 
                entities.GetUnsafePtr(), 
                UnsafeUtility.SizeOf<TargetEntity>() * entities.Length);
        }
    }
}
