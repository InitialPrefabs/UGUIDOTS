using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace UGUIDots.Render.Systems {
    [UpdateInGroup(typeof(MeshUpdateGroup))]
    public class ApplyMaterialColorSystem : SystemBase {

        private struct RecurseJob {
 
            [ReadOnly]
            public BufferFromEntity<Child> ChildrenBuffer;

            [ReadOnly]
            public ComponentDataFromEntity<AppliedColor> Colors;

            [ReadOnly]
            public ComponentDataFromEntity<MaterialPropertyIndex> Indices;

            public void RecurseApplyUpdate(Entity parent, MaterialPropertyBatch batch) {
                if (!ChildrenBuffer.Exists(parent)) {
                    return;
                }

                var children = ChildrenBuffer[parent].AsNativeArray();

                for (int i = 0; i < children.Length; i++) {
                    var current = children[i].Value;

                    // TODO: Maybe add the check...
                    if (Colors.Exists(current) && Indices.Exists(current)) {
                        var idx = Indices[current].Value;
                    }
                }
            }
        }

        protected override void OnUpdate() {
            var childrens = GetBufferFromEntity<Child>(true);
            Entities.ForEach((Entity entity, DynamicBuffer<RenderElement> b0, MaterialPropertyBatch c0) => {
            }).WithoutBurst().Run();
        }
    }
}
