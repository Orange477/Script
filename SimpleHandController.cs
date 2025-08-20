using UnityEngine;

public class SimpleHandController : MonoBehaviour
{
    [Header("Depth Mapping")]
    public float zMin = 0.3f;       // ��v���e 0.3m
    public float zMax = 1.5f;       // ��v���e 1.5m
    public float zRawMin = -0.02f;  // MediaPipe �̾a��۾��� z ��
    public float zRawMax = -0.20f;  // MediaPipe �̻����۾��� z ��

    [Header("Depth Sensitivity Curve")]
    [Tooltip("X �b�G�`�ס]0��zMin, 1��zMax�^�AY �b�G��j���ơC")]
    public AnimationCurve sensitivityCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Header("Hand Anchor (������)")]
    public Transform anchor; // �� �o�̩�J Anchor �Ū���

    [Header("�����L��")]
    public Vector3 manualOffset = Vector3.zero; // ���M�s�A�קK�v�T�Z���P�_ 

    [Header("Z ����ﶵ")]
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
            // ��@�ɮy���ন�۹� Anchor ���y��
            Vector3 localPos = anchor.InverseTransformPoint(worldPos);

            // �[�W��ҫ�����ʷL��
            transform.localPosition = localPos + manualOffset;
        }
        else
        {
            // �S�] Anchor�A�����Υ@�ɮy��
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
