using UnityEngine;

namespace UGUIDOTS.Analyzers {

    [RequireComponent(typeof(Canvas))]
    public class BakedCanvasRunner : MonoBehaviour {
        public BakedCanvasData BakedCanvasData;

        public int Index = -1;
    }
}
