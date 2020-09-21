using UnityEngine;

namespace UGUIDOTS.Analyzers {

    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class BakedCanvasDataProxy : MonoBehaviour {

        public BakedCanvasData BakedCanvasData;

        public int Index = -1;
    }
}
