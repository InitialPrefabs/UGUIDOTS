using UnityEngine;

public class ConstantScreenSizeLogger : MonoBehaviour {

    void Update() {
        Debug.Log($"[Screen] Width: {Screen.width}, Height: {Screen.height}");
    }
}
