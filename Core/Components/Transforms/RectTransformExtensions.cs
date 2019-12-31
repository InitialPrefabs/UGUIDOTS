using Unity.Mathematics;
using UnityEngine;

namespace UGUIDots.Transforms {

    public static class RectTransformExtensions {

        public static void AnchorOf(this RectTransform transform, out float2 distance, out AnchoredState anchor) {
            anchor = transform.ToAnchor();
            distance = anchor.AnchoredTo() - new float2(transform.position.x, transform.position.y);
        }
    }
}
