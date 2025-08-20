using UnityEngine;

public class SimpleHandController_Scene2 : MonoBehaviour
{
    [Header("固定深度位置")]
    public float fixedDepth = 1.2f;

    [Header("手動偏移 (local)")]
    public Vector3 manualOffset = new Vector3(0f, -0.1f, 0f);

    public void ApplyLandmarks(HandManager.Landmark[] landmarks)
    {
        if (landmarks == null || landmarks.Length < 1) return;

        var l = landmarks[8]; // index tip
        float x = Mathf.Clamp01(l.x);
        float y = Mathf.Clamp01(1f - l.y);
        Vector3 vp = new Vector3(x, y, fixedDepth);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(vp);
        transform.position = worldPos + manualOffset;
    }
}