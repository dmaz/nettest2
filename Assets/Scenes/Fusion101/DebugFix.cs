using UnityEngine;
using UnityEngine.Rendering;

public class DebugFix : MonoBehaviour
{
    void Awake() {
        DebugManager.instance.enableRuntimeUI = false;
    }
}
