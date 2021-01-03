using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIDOTS.Analyzers {

    [Serializable]
    public class CanvasTransform {

        public Vector2 WPosition => WorldPosition;
        public Vector2 WScale    => WorldScale;
        public Vector2 LPosition => LocalPosition;
        public Vector2 LScale    => LocalScale;

        public string Name;

        // Store the LocalToWorld info
        public Vector3 WorldPosition;
        public Vector3 WorldScale;

        // Store the LocalToParent info
        public Vector3 LocalPosition;
        public Vector3 LocalScale;

        public List<CanvasTransform> Children;

        public CanvasTransform() {
            Children = new List<CanvasTransform>();
        }

        public CanvasTransform(
            Vector3 worldPos, 
            Vector3 worldScale, 
            Vector3 localPos, 
            Vector3 localScale,
            string name) {

            WorldPosition = worldPos;
            WorldScale    = worldScale;
            LocalPosition = localPos;
            LocalScale    = localScale;
            Name          = name;

            Children = new List<CanvasTransform>();
        }
    }

    [Serializable]
    public sealed class RootCanvasTransform : CanvasTransform {

        public int InstanceID;

        public RootCanvasTransform(
            Vector3 worldPos, 
            Vector3 worldScale, 
            Vector3 localPos, 
            Vector3 localScale,
            int instanceID,
            string name) : base(worldPos, worldScale, localPos, localScale, name) {
            InstanceID = instanceID;
        }
    }

    [CreateAssetMenu(menuName = "UGUIDOTS/BakedCanvasInformation", fileName = "Baked Canvas Data")]
    public class BakedCanvasData : ScriptableObject, ISerializationCallbackReceiver {

        public List<RootCanvasTransform> Hierarchy;

        private Dictionary<int, RootCanvasTransform> canvasMap = new Dictionary<int, RootCanvasTransform>();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }

        public bool ElementAt(int id, out RootCanvasTransform transform) {
            var index = Hierarchy.FindIndex((element) => { return element.InstanceID == id; });

            if (index > -1) {
                transform = Hierarchy[index];
                return true;
            }

            transform = null;
            return false;
        }

        public RootCanvasTransform this[int id] {
            get {
                if (ElementAt(id, out RootCanvasTransform transform)) {
                    return transform;
                }
                return null;
            }
        }
    }
}
