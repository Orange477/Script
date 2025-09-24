using UnityEngine;
using Leap.Unity.Interaction;
using Leap;

public class GrabTrigger : MonoBehaviour
{
    [Header("手部與手模型")]
    public InteractionHand hand; // 指定 InteractionHand
    public Transform grabPoint;  // 抓取位置

    [Header("抓取設定")]
    public string[] grabTags = new string[] { "Ladle", "egg" };
    public float grabRange = 0.4f;
    public Vector3 grabOffset = Vector3.zero;
    public Vector3 grabRotationEuler = Vector3.zero;

    public GameObject heldObject;
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
        string name = "無";    
        if (grabPoint.childCount > 0)
        {
            bool OBJInGrabPoint()
            {
                for (int i = 0; i < grabPoint.childCount; i++)
                {
                    Transform child = grabPoint.GetChild(i);
                    foreach (var tag in grabTags)
                    {
                        if (child.CompareTag(tag) || child.name.ToLower().Contains(tag))
                        {
                            name = child.name;
                            return true;
                        }
                    }
                }
                return false;
            }
            if (OBJInGrabPoint())
            {
                Debug.Log("目前抓著的物件：" + name);
            }

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
            /*heldObject.transform.position = grabPoint.position + grabOffset;
            heldObject.transform.rotation = Quaternion.Euler(grabRotationEuler);*/
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay 進入：" + other.name + "，Tag：" + other.tag);
        if (heldObject != null || isHolding) return;

        Hand leapHand = hand.leapHand;
        if (leapHand == null || leapHand.GrabStrength < 0.8f) return;

        Transform current = other.transform;
        GameObject target = null;
        while (current != null)
        {
            // 判斷 tag 是否在 grabTags 陣列裡
            foreach (var tag in grabTags)
            {
                if (current.CompareTag(tag))
                {
                    target = current.gameObject;
                    break;
                }
            }
            if (target != null) break;
            current = current.parent;
        }

        if (target == null) return;

        float distance = Vector3.Distance(grabPoint.position, target.transform.position);
        if (distance > grabRange) return;  //這裡有問題9/24

        GrabObject(target);
        isHolding = true;
    }


    void GrabObject(GameObject obj)
    {
        Debug.Log("GrabObject 被呼叫：" + obj.name + "，Tag：" + obj.tag);
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();
        Debug.Log("GrabObject後 heldObject: " + heldObject);

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
         Debug.Log("ReleaseObject前 heldObject: " + heldObject);

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
        isHolding = false; // <--- 加這行
    }
}