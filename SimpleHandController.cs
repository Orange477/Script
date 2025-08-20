using UnityEngine;

public class SimpleHandController : MonoBehaviour
{
    [Header("Depth Mapping")]
    public float zMin = 0.3f;       // 攝影機前 0.3m
    public float zMax = 1.5f;       // 攝影機前 1.5m
    public float zRawMin = -0.02f;  // MediaPipe 最靠近相機的 z 值
    public float zRawMax = -0.20f;  // MediaPipe 最遠離相機的 z 值

    [Header("Depth Sensitivity Curve")]
    [Tooltip("X 軸：深度（0→zMin, 1→zMax），Y 軸：放大倍數。")]
    public AnimationCurve sensitivityCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Header("Hand Anchor (父物件)")]
    public Transform anchor; // ← 這裡拖入 Anchor 空物件

    [Header("偏移微調")]
    public Vector3 manualOffset = Vector3.zero; // 先清零，避免影響距離判斷 

    [Header("Z 控制選項")]
    public bool forceFixedDepth = false;
    public float fixedDepth = 1.0f;

    public void ApplyLandmarks(HandManager.Landmark[] landmarks)
    {
        if (landmarks == null || landmarks.Length < 1) return;

        int indexTip = 8;
        var l = (landmarks.Length > indexTip) ? landmarks[indexTip] : landmarks[0];

        Vector3 worldPos = AdjustToCameraSpace(l.x, l.y, l.z);

        if (anchor != null)
        {
            // 把世界座標轉成相對 Anchor 的座標
            Vector3 localPos = anchor.InverseTransformPoint(worldPos);

            // 加上手模型的手動微調
            transform.localPosition = localPos + manualOffset;
        }
        else
        {
            // 沒設 Anchor，直接用世界座標
            transform.position = worldPos + manualOffset;

            Debug.Log($"[ApplyLandmarks] {gameObject.name} worldPos = {transform.position}");
        }
    }

    private Vector3 AdjustToCameraSpace(float mpX, float mpY, float mpZ)
    {
        float finalDepth;

        if (forceFixedDepth)
        {
            finalDepth = fixedDepth;
        }
        else
        {
            float rawNorm = Mathf.InverseLerp(zRawMin, zRawMax, mpZ);
            float zNorm = 1f - rawNorm;

            float baseDepth = Mathf.Lerp(zMin, zMax, zNorm);
            float depthCenter = (zMin + zMax) * 0.5f;

            float depthNormalized = (baseDepth - zMin) / (zMax - zMin);
            float sensitivity = sensitivityCurve.Evaluate(depthNormalized);

            finalDepth = depthCenter + (baseDepth - depthCenter) * sensitivity;
        }

        float x = Mathf.Clamp01(mpX);
        float y = Mathf.Clamp01(1f - mpY);

        Vector3 viewportPos = new Vector3(x, y, finalDepth);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewportPos);

        return worldPos;
    }
}
