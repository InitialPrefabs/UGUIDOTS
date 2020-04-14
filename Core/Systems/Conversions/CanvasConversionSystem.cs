using System;
using System.Collections.Generic;
using TMPro;
using UGUIDots.Render;
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

        private List<MaterialPropertyBlock> blocks;
        public Dictionary<int, int> blockMap;

        protected override void OnCreate() {
            base.OnCreate();

            blocks = new List<MaterialPropertyBlock>();
            blockMap = new Dictionary<int, int>();
        }

        protected override void OnUpdate() {
            Entities.ForEach((Canvas canvas) => {

                var parent = canvas.transform.parent;
                if (parent != null) {
#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(canvas);
#endif
                    throw new NotSupportedException($"{canvas.name} is child of {parent.name}, this will not be " + 
                        "supported!");
                }

                CreateMaterialPropertyBatch(blocks, blockMap, canvas.transform);

                var entity       = GetPrimaryEntity(canvas);
                var canvasScaler = canvas.GetComponent<CanvasScaler>();

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

                DstEntityManager.AddComponentData(entity, new MaterialPropertyBatch {
                    Value = blocks.ToArray()
                });

                blocks.Clear();
                blockMap.Clear();
            });
        }

        private void CreateMaterialPropertyBatch(List<MaterialPropertyBlock> blocks, Dictionary<int, int> map, 
            Transform parent) {

            for (int i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);

                if (child.TryGetComponent(out Image image)) {
                    var imgEntity = GetPrimaryEntity(image);
                    var mat = image.material != null ? image.material : Canvas.GetDefaultCanvasMaterial();

                    if (blockMap.TryGetValue(mat.GetHashCode(), out var idx)) {
                        DstEntityManager.AddComponentData(imgEntity, new MaterialPropertyIndex {
                            Value = (ushort)idx
                        });
                    } else {
                        var block = new MaterialPropertyBlock { };
                        blocks.Add(block);
                        idx = blocks.Count - 1;
                        blockMap.Add(mat.GetHashCode(), idx);

                        DstEntityManager.AddComponentData(imgEntity, new MaterialPropertyIndex {
                            Value = (ushort)idx
                        });
                    }
                }

                if (child.TryGetComponent(out TextMeshProUGUI text)) {
                    var textEntity = GetPrimaryEntity(text);
                    var mat = text.materialForRendering != null ? text.materialForRendering : 
                        Canvas.GetDefaultCanvasMaterial();

                    if (blockMap.TryGetValue(mat.GetHashCode(), out var idx)) {
                        DstEntityManager.AddComponentData(textEntity, new MaterialPropertyIndex {
                            Value = (ushort)idx
                        });
                    } else {
                        var block = new MaterialPropertyBlock { };
                        blocks.Add(block);
                        idx = blocks.Count - 1;
                        blockMap.Add(mat.GetHashCode(), idx);

                        DstEntityManager.AddComponentData(textEntity, new MaterialPropertyIndex {
                            Value = (ushort)idx
                        });
                    }
                }

                if (child.childCount > 0) {
                    CreateMaterialPropertyBatch(blocks, map, child);
                }
            }
        }
    }
}
