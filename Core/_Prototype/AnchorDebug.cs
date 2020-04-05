using System.Collections;
using System.Collections.Generic;
using UGUIDots.Transforms;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class AnchorDebug : MonoBehaviour {

    // Update is called once per frame
    void Update() {
        var anchor = ((RectTransform)transform).ToAnchor();
        Debug.Log($"World Position {name}: {transform.position}, Anchor: {((RectTransform)transform).anchorMin}");
        Debug.Log($"Projected Anchor {name}: {anchor} {anchor.AnchoredTo(new int2(Screen.height, Screen.width))}");
    }
}
