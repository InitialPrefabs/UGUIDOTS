using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace UGUIDOTS {

    public class CursorAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

        [Tooltip("If you only need the mouse then a cursor size of 1 makes sense, for mobile use 2 or greater")]
        public ushort CursorSize = 1;

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            var cursors = dstManager.AddBuffer<Cursor>(entity);
            cursors.ResizeUninitialized(CursorSize);
            UnsafeUtility.MemSet(cursors.GetUnsafePtr(), 0, CursorSize * UnsafeUtility.SizeOf<Cursor>());
            dstManager.AddComponent<CursorTag>(entity);
        }
    }
}
