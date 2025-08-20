using UnityEngine;
using System.Collections.Generic;

public class GrabTrigger : MonoBehaviour
{
    public float grabRange = 10f; // 抓取距離
    public Transform handTransform; // 手的 Transform
    public HandManager handManager; // 手部管理器，提供手指點資訊

    private GameObject heldObject = null;
    private static GameObject globallyHeldObject = null; // 全局唯一被抓物件
    private Dictionary<GameObject, (Vector3 pos, Quaternion rot)> originalTransforms = new Dictionary<GameObject, (Vector3, Quaternion)>();

    private void Start()
    {
        // 事先找到所有有 Ladle 標籤的物品，記錄他們的初始位置和旋轉
        var ladles = GameObject.FindGameObjectsWithTag("Ladle");
        foreach (var ladle in ladles)
        {
            originalTransforms[ladle] = (ladle.transform.position, ladle.transform.rotation);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (heldObject != null) return; // 已抓取物品時不繼續抓

        Transform current = other.transform;
        GameObject target = null;

        // 找到有 Ladle 標籤的父物件
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

        Vector3 handPos = handTransform.position;
        Vector3 targetPos = target.transform.position;

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null) return;

        Vector3 closestPointOnTarget = targetCollider.ClosestPoint(handPos);
        float distance = Vector3.Distance(handPos, closestPointOnTarget);

        if (distance > grabRange) return; // 超過抓取範圍不抓

        if (IsPinching())
        {
            GrabObject(target);
        }
    }

    private void Update()
    {
        // 放開物品條件：已抓且不再捏合
        if (heldObject != null && !IsPinching())
        {
            ReleaseObject();
        }
    }

    private void GrabObject(GameObject target)
    {
        if (globallyHeldObject != null) return; // 其他手已抓物件時跳過

        heldObject = target;
        globallyHeldObject = target;

        heldObject.transform.SetParent(handTransform);

        // 物品位置歸零（相對手的位置）
        heldObject.transform.localPosition = Vector3.zero;

        // 物品旋轉設定為跟手同步（可微調）
        heldObject.transform.rotation = handTransform.rotation;
        // 若物品方向錯，可以試試以下微調角度
        heldObject.transform.rotation = handTransform.rotation * Quaternion.Euler(-90, 0, 0);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("抓取：" + heldObject.name);
    }

    private void ReleaseObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("放開：" + heldObject.name);

        heldObject = null;
        globallyHeldObject = null;
    }

    private bool IsPinching()
    {
        if (handManager == null) return false;

        var landmarks = handManager.GetLandmarks();
        if (landmarks == null || landmarks.Length < 9) return false;

        Vector3 thumb = new Vector3(landmarks[4].x, landmarks[4].y, landmarks[4].z);
        Vector3 index = new Vector3(landmarks[8].x, landmarks[8].y, landmarks[8].z);

        float pinchDist = Vector3.Distance(thumb, index);

        Debug.Log($"拇指與食指距離: {pinchDist}, 是否抓取: {pinchDist < 0.3f}");

        return pinchDist < 0.3f;
    }
}
