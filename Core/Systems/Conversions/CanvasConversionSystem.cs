using System;
using UGUIDots.Render;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    [UpdateAfter(typeof(RectTransformConversionSystem))]
    [UpdateAfter(typeof(ImageConversionSystem))]
    [UpdateAfter(typeof(TMPTextConversionSystem))]
    public class CanvasConversionSystem : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {

                var parent = canvas.transform.parent;
                if (parent != null) {
#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(canvas);
#endif
                    throw new NotSupportedException($"{canvas.name} is child of {parent.name}, this is not supported!");
                }

                var entity       = GetPrimaryEntity(canvas);
                var canvasScaler = canvas.GetComponent<CanvasScaler>();

                // Build a metadata of the children which are active
                var count = 0;
                count     = RecurseCountChildren(ref count, canvas.transform);

                var metadata = new UnsafeHashMap<Entity, bool>(count + 1, Allocator.Persistent);
                RecurseAddMetadata(ref metadata, canvas.transform);

                DstEntityManager.AddComponentData(entity, new ChildrenActiveMetadata {
                    Value = metadata
                });

                // Remove unnecessary information
                DstEntityManager.RemoveComponent<Rotation>(entity);
                DstEntityManager.RemoveComponent<Translation>(entity);
                DstEntityManager.RemoveComponent<NonUniformScale>(entity);

                // Add the root mesh renderering data to the canvas as the root primary renderer
                DstEntityManager.AddBuffer<RootVertexData>(entity);
                DstEntityManager.AddBuffer<RootTriangleIndexElement>(entity);

                // Add a mesh to the canvas so treat it as a renderer.
                DstEntityManager.AddComponentData(entity, new AddMeshTag { });

                // Add a collection of the submesh information
                DstEntityManager.AddBuffer<SubmeshSliceElement>(entity);
                DstEntityManager.AddBuffer<SubmeshKeyElement>(entity);

                switch (canvasScaler.uiScaleMode) {
                    case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                        DstEntityManager.AddComponentData(entity, new ReferenceResolution {
                            Value = canvasScaler.referenceResolution
                        });

                        // TODO: Should figure out if I want to support shrinking and expanding only...
                        if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight) {
                            DstEntityManager.AddComponentData(entity, new WidthHeightRatio {
                                Value =  canvasScaler.matchWidthOrHeight
                            });
                        } else {
                            throw new NotSupportedException($"{canvasScaler.screenMatchMode} is not supported yet.");
                        }
                        break;
                    default:
                        throw new NotSupportedException($"{canvasScaler.uiScaleMode} is not supported yet.");
                }
            });
        }

        private int RecurseCountChildren(ref int count, Transform current) {
            count += current.childCount;
            for (int i = 0; i < current.childCount; i++) {
                var child = current.GetChild(i);

                if (child.childCount > 0) {
                    RecurseCountChildren(ref count, child);
                }
            }
            return count;
        }

        private void RecurseAddMetadata(ref UnsafeHashMap<Entity, bool> interactableMetadata, Transform current) {
            for (int i = 0; i < current.childCount; i++) {
                var child       = current.GetChild(i);
                var childEntity = this.GetPrimaryEntity(child);

                interactableMetadata.TryAdd(GetPrimaryEntity(child), child.gameObject.activeSelf);

                if (child.childCount > 0) {
                    RecurseAddMetadata(ref interactableMetadata, child);
                }
            }
        }
    }
}
