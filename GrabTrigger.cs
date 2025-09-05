using UnityEngine;

public class GrabTrigger : MonoBehaviour
{
    [Header("抓取設定")]
    public string grabTag = "Ladle"; // Inspector 設定抓取物件的 Tag
    public float grabRange = 0.4f;   // 抓取距離
    public Transform handTransform;  // 手的位置
    public HandManager handManager;  // Mediapipe 的 handManager

    [Header("抓取點（放在手心）")]
    public Transform grabPoint;      // 手心抓取點，用空物件放在手心

    [Header("抓取後的微調")]
    public Vector3 grabOffset = Vector3.zero;      // 抓取時的位置偏移
    public Vector3 grabRotationEuler = Vector3.zero; // 抓取時的旋轉調整

    public GameObject heldObject = null;
    public static GameObject globallyHeldObject = null;

    public Vector3 originalPos;
    public Quaternion originalRot;

    public void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay 進入：" + other.name);
        if (heldObject != null) return;

        Transform current = other.transform;
        GameObject target = null;
        while (current != null)
        {
            if (current.CompareTag(grabTag)) // ✅ 用 Inspector 設定的 Tag
            {
                target = current.gameObject;
                break;
            }
            current = current.parent;
        }

        if (target == null) return;

        Vector3 handPos = grabPoint.position;
        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null) return;

        Vector3 closestPointOnTarget = targetCollider.ClosestPoint(handPos);
        float distance = Vector3.Distance(handPos, closestPointOnTarget);

         Debug.Log($"手心到蛋的距離: {distance}");
        if (distance > grabRange) return;

        if (IsFist()) GrabObject(target);
    }

    public void Update()
    {
        float avgDist = GetFingerAvgDist();
        Debug.Log($"{handTransform.name} 手指平均距離: {avgDist}, 握拳: {IsFist()}, 張開: {IsPalmOpen()}");

        if (heldObject != null && IsPalmOpen())
        {
            ReleaseObject();
        }
    }

    public void GrabObject(GameObject target)
    {
        Debug.Log("GrabObject 被呼叫：" + target.name);
        if (globallyHeldObject != null) return;

        heldObject = target;
        globallyHeldObject = target;

        originalPos = heldObject.transform.position;
        originalRot = heldObject.transform.rotation;

        // ✅ 把物品 attach 到抓取點
        heldObject.transform.SetParent(grabPoint);

        // ✅ 應用 Inspector 的偏移與旋轉
        heldObject.transform.localPosition = grabOffset;
        heldObject.transform.localRotation = Quaternion.Euler(grabRotationEuler);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Debug.Log("抓取：" + heldObject.name);
    }

    public void ReleaseObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        heldObject.transform.position = originalPos;
        heldObject.transform.rotation = originalRot;

        Debug.Log("放開並回到原位：" + heldObject.name);

        heldObject = null;
        globallyHeldObject = null;
    }

    public float GetFingerAvgDist()
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

    public bool IsFist() => GetFingerAvgDist() < 0.25f;
    public bool IsPalmOpen() => GetFingerAvgDist() > 0.35f;
}
