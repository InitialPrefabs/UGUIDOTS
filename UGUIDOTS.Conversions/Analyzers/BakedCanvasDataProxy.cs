using UnityEngine;

namespace UGUIDOTS.Analyzers {

    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class BakedCanvasDataProxy : MonoBehaviour {

        public int InstanceID {
            get { return instanceID; }
        }

        public BakedCanvasData BakedCanvasData;

        [System.Obsolete]
        public int Index = -1;
        
#pragma warning disable CS0649
        [SerializeField]
        [HideInInspector]
        private int instanceID;
#pragma warning restore CS0649

        public RootCanvasTransform Root => BakedCanvasData[instanceID];
    }
}
