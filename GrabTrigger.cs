using UnityEngine;
using Leap.Unity.Interaction;
using Leap;

public class GrabTrigger : MonoBehaviour
{
    [Header("手部與手模型")]
    public InteractionHand hand; // 指定 InteractionHand
    public Transform grabPoint;  // 抓取位置

    [Header("抓取設定")]
    public string grabTag = "Ladle";
    public float grabRange = 0.4f;
    public Vector3 grabOffset = Vector3.zero;
    public Vector3 grabRotationEuler = Vector3.zero;

    public static GameObject heldObject ;
    public Rigidbody heldRb = null;
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public Transform originalParent;

    private bool isHolding = false;
    private float releaseCooldown = 0.2f;
    private float releaseTimer = 0f;


    void Start()
    {
        InteractionBehaviour ib = GetComponent<InteractionBehaviour>();
        if (ib != null)
        {
            ib.throwHandler = null;       // 停用丟擲邏輯
            ib.ignoreGrasping = true;     // 停用 SDK 的自動抓取
        }
    }


    void Update()
    {
         Hand leapHand = hand.leapHand;

    if (heldObject != null)
    {
        if (leapHand != null)
            {
                if (leapHand.GrabStrength < 0.8f)
                {
                    releaseTimer += Time.deltaTime;
                    if (releaseTimer > releaseCooldown)
                    {
                        ReleaseObject();
                        isHolding = false;
                        releaseTimer = 0f;
                        return;
                    }
                }
                else
                {
                    releaseTimer = 0f; // 重置計時器
                }
            }

        // 穩定吸附在手部指定點
        heldObject.transform.position = grabPoint.position + grabOffset;
        heldObject.transform.rotation = Quaternion.Euler(grabRotationEuler);
    }

    }

    private void OnTriggerStay(Collider other)
    {
        if (heldObject != null || isHolding) return;

        Hand leapHand = hand.leapHand;
        if (leapHand == null || leapHand.GrabStrength < 0.8f) return;

        Transform current = other.transform;
        GameObject target = null;
        while (current != null)
        {
            if (current.CompareTag(grabTag))
            {
                target = current.gameObject;
                break;
            }
            current = current.parent;
        }

        if (target == null) return;

        float distance = Vector3.Distance(grabPoint.position, target.transform.position);
        if (distance > grabRange) return;

        GrabObject(target);
        isHolding = true;
    }


    void GrabObject(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        // ✅ 記錄原始世界座標與父物件
        originalPosition = obj.transform.position;
        originalRotation = obj.transform.rotation;
        originalParent = obj.transform.parent;

        if (heldRb != null)
        {
            heldRb.velocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.isKinematic = true; // ✅ 禁用物理，穩定吸附
        }

        // ✅ 吸附到手部指定點
        heldObject.transform.SetParent(grabPoint);
        heldObject.transform.localPosition = grabOffset;
        heldObject.transform.localRotation = Quaternion.Euler(grabRotationEuler);
        heldObject.transform.SetParent(grabPoint, worldPositionStays: false);

        Debug.Log("抓取並吸附：" + heldObject.name);
    }


    public void ReleaseObject()
    {
        if (heldObject == null) return;
        {
            heldObject.transform.SetParent(null); // 解除抓取點
            heldObject.transform.position = originalPosition;
            heldObject.transform.rotation = originalRotation;
            heldObject.transform.SetParent(originalParent); // 還原階層

        }

        if (heldRb != null)
        {
            heldRb.velocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.isKinematic = true; // ✅ 保持 kinematic，不掉落
        }

        Debug.Log("放開物件並回原位：" + heldObject.name);

        heldObject = null;
        heldRb = null;
        originalParent = null;

    }
}