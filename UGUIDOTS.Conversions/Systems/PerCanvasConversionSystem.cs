using System.Collections.Generic;
using UGUIDOTS.Analyzers;
using UGUIDOTS.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDOTS.Conversions.Systems {

    internal class PerCanvasConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            var canvasMap = new Dictionary<Entity, int2>();
            Entities.ForEach((Canvas canvas) => {
                var canvasEntity = GetPrimaryEntity(canvas);
                DstEntityManager.RemoveComponent<Anchor>(canvasEntity);

                var canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasMap.Add(canvasEntity, new int2(canvasScaler.referenceResolution));

                CanvasConversionUtils.CleanCanvas(canvasEntity, DstEntityManager);
                CanvasConversionUtils.SetScaleMode(canvasEntity, canvas, DstEntityManager, canvasScaler);

                if (canvas.TryGetComponent(out BakedCanvasDataProxy proxy)) {
                    var exists = proxy.BakedCanvasData.ElementAt(proxy.InstanceID, out RootCanvasTransform hierarchy);
                    var root   = canvas.transform;

#if UNITY_EDITOR
                    if (!exists) {
                        Debug.LogWarning($"<b>{canvas.name}</b> is not baked, skipping for now...");
                        return;
                    }
#endif

                    DstEntityManager.AddComponentData(canvasEntity, new ScreenSpace {
                        Scale       = hierarchy.WScale,
                        Translation = hierarchy.WPosition
                    });

                    RecurseChildren(root, hierarchy);
                }
            });
        }

        private unsafe void RecurseChildren(Transform parent, CanvasTransform parentData) {
            var children = new Child[parent.childCount];
            for (int i = 0; i < parent.childCount; i++) {
                var child           = parent.GetChild(i);
                var associatedXform = parentData.Children[i];
                var childEntity     = GetPrimaryEntity(child);

                DstEntityManager.SetComponentData(childEntity, new ScreenSpace {
                    Scale       = associatedXform.WScale,
                    Translation = associatedXform.WPosition
                });

                DstEntityManager.SetComponentData(childEntity, new LocalSpace {
                    Scale       = associatedXform.LScale,
                    Translation = associatedXform.LPosition
                });

                children[i] = childEntity;

                if (parent.childCount > 0) {
                    RecurseChildren(child, associatedXform);
                }
            }

            if (children.Length > 0) {
                // Add all the parents to the children
                foreach (var child in children) {
                    DstEntityManager.AddComponentData(child.Value, new Parent { Value = GetPrimaryEntity(parent) });
                }

                var buffer = DstEntityManager.AddBuffer<Transforms.Child>(GetPrimaryEntity(parent));
                buffer.ResizeUninitialized(children.Length);


                fixed (Transforms.Child* ptr = children) {
                    UnsafeUtility.MemCpy(
                        buffer.GetUnsafePtr(), 
                        ptr,
                        UnsafeUtility.SizeOf<Entity>() * children.Length);
                }
            }
        }
    }
}
