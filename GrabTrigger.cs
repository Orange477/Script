using UnityEngine;

public class GrabTrigger : MonoBehaviour
{
    public string grabTag = "Ladle"; // Inspector 設定抓取物件的 Tag
    public Vector3 grabRotationEuler = new Vector3(0, 0, 0);
    public float grabRange = 0.4f; // 抓取距離
    public Transform handTransform; // 手的位置
    public HandManager handManager;  // Mediapipe 的 handManager

    [Header("抓取點")]
    public Transform grabPoint; // 手心抓取點，用空物件放在手心

    private GameObject heldObject = null;
    private static GameObject globallyHeldObject = null;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private void OnTriggerStay(Collider other)
    {
        if (heldObject != null) return; // 已抓取物品就不抓

        // 找標籤為 Ladle 的父物件
        Transform current = other.transform;
        GameObject target = null;
        while (current != null)
        {
            if (current.CompareTag("Ladle"))
            {
                target = current.gameObject;
                break;
            }
            current = current.parent;
        }

        if (target == null) return;

        Vector3 handPos = grabPoint.position; // 用抓取點判定距離
        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null) return;

        Vector3 closestPointOnTarget = targetCollider.ClosestPoint(handPos);
        float distance = Vector3.Distance(handPos, closestPointOnTarget);

        if (distance > grabRange) return;

        // 握拳抓取
        if (IsFist())
        {
            GrabObject(target);
        }
    }

    private void Update()
    {
        // 每幀輸出手部狀態
        float avgDist = GetFingerAvgDist();
        Debug.Log($"{handTransform.name} 手指平均距離: {avgDist}, 握拳: {IsFist()}, 張開: {IsPalmOpen()}");

        // 張開放下
        if (heldObject != null && IsPalmOpen())
        {
            ReleaseObject();
        }
    }

    private void GrabObject(GameObject target)
    {
        if (globallyHeldObject != null) return;

        heldObject = target;
        globallyHeldObject = target;

        originalPos = heldObject.transform.position;
        originalRot = heldObject.transform.rotation;

        // 把物品 attach 到抓取點
        heldObject.transform.SetParent(grabPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        // 使用 Inspector 可調旋轉
        heldObject.transform.localRotation = Quaternion.Euler(grabRotationEuler);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Debug.Log("抓取：" + heldObject.name);
    }

    private void ReleaseObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false; // 允許掉落或受物理影響

        // 回到原本位置
        heldObject.transform.position = originalPos;
        heldObject.transform.rotation = originalRot;

        Debug.Log("放開並回到原位：" + heldObject.name);

        heldObject = null;
        globallyHeldObject = null;
    }

    private float GetFingerAvgDist()
    {
        var landmarks = handManager.GetLandmarks(handTransform.name.Contains("Left"));
        if (landmarks == null || landmarks.Length < 21) return 0f;

        Vector3 palm = new Vector3(landmarks[0].x, landmarks[0].y, landmarks[0].z);
        float sum = 0f;
        int[] tips = { 8, 12, 16, 20 };
        foreach (int tip in tips)
        {
            Vector3 tipPos = new Vector3(landmarks[tip].x, landmarks[tip].y, landmarks[tip].z);
            sum += Vector3.Distance(palm, tipPos);
        }
        return sum / tips.Length;
    }

    private bool IsFist()
    {
        float avgDist = GetFingerAvgDist();
        return avgDist < 0.25f;
    }

    private bool IsPalmOpen()
    {
        float avgDist = GetFingerAvgDist();
        return avgDist > 0.35f;
    }
}
