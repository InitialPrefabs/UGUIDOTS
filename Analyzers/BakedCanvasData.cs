using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIDOTS.Analyzers {
    
    [CreateAssetMenu(menuName = "UGUIDOTS/BakedCanvasInformation", fileName = "Baked Canvas Data")]
    public class BakedCanvasData : ScriptableObject {
        
        [Serializable]
        public struct CanvasTransform {
            public int InstanceID;
            public Vector3 Translation;
            public Vector3 Scale;

            public Matrix4x4 AsFloat4x4() {
                return Matrix4x4.TRS(Translation, Quaternion.identity, Scale);
            }
        }

        public List<CanvasTransform> CanvasTransforms;
    }
}
