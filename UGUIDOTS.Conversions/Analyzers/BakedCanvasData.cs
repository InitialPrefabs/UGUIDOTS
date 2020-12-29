using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIDOTS.Analyzers {
    
    [CreateAssetMenu(menuName = "UGUIDOTS/BakedCanvasInformation", fileName = "Baked Canvas Data")]
    public class BakedCanvasData : ScriptableObject {
        
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

        public List<CanvasTransform> Transforms;
    }
}
