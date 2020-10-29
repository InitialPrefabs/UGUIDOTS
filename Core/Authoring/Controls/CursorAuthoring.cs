using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS {

    public class CursorAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        [Tooltip("If you only need the mouse then a cursor size of 1 makes sense, for mobile use 2 or greater")]
        public ushort CursorSize = 1;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var buffer = dstManager.AddBuffer<Cursor>(entity);
            buffer.ResizeUninitialized(1);

            unsafe {
                UnsafeUtility.MemSet(buffer.GetUnsafePtr(), default, UnsafeUtility.SizeOf<Cursor>());
            }
        }
    }
}
