using UnityEngine;
using Leap.Unity.Interaction;
using Leap;

public class GrabTrigger : MonoBehaviour
{
    [Header("手部與手模型")]
    public InteractionHand hand; // 指定 InteractionHand
    public Transform grabPoint;  // 抓取位置

    [Header("抓取設定")]
    public string[] grabTags = new string[] {"Ladle","egg"};
    public float grabRange = 0.4f;
    public Vector3 grabOffset = Vector3.zero;
    public Vector3 grabRotationEuler = Vector3.zero;

    public  GameObject heldObject;
    public Rigidbody heldRb = null;
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public Transform originalParent;
    public Collider other;
    public bool isHolding = false;
    public bool isHoldingEGG = false;
    public bool isHoldingLADLE = false;
    private float releaseCooldown = 0.2f;
    private float releaseTimer = 0f;
    private Vector3 originalScale;

    void Start()
    {
        Debug.Log("1");
        InteractionBehaviour ib = GetComponent<InteractionBehaviour>();
        if (ib != null)
        {
            ib.throwHandler = null;       // 停用丟擲邏輯
            ib.ignoreGrasping = true;     // 停用 SDK 的自動抓取
        }
    }


    public void Update()
    {
        
        string name = "無";
        if (grabPoint.childCount > 0 && isHolding == true)
        {
            Hand leapHand = hand.leapHand;
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
                            releaseTimer = 0f;
                            return;
                        }
                    }
                    else
                    {
                        releaseTimer = 0f; // 重置計時器1
                    }
                }
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        Hand leapHand = hand.leapHand;
        Transform current = other.transform;
        GameObject target = null;
        
        while (current != null)
        {
            for (int i = 0; i < grabTags.Length; i++)
            {
                string tag = grabTags[i];
                Debug.Log("正在檢查 Tag：" + tag + " 是否與目前物件 " + current.name + " 的 Tag " + current.tag + " 相符");
                if (tag == current.tag)
                {
                    target = current.gameObject;
                    break;
                }
            }
            if (target != null)
            {
                Debug.Log("找到目標物件：" + target.name + "，Tag：" + target.tag);
                break;
            }
            else
            {
                Debug.Log("目前物件 " + current.name + " 的 Tag " + current.tag + " 不在抓取列表中");
            }
            current = current.parent;
        }
        if (target == null) return;
        if (isHoldingEGG == true && current.name == "egg") return;
        if( isHoldingLADLE == true && current.name == "Ladle") return;
        GrabObject(target);
    }


    public void GrabObject(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();
        if (isHolding == true && heldObject.name == "egg")
        {
            isHoldingEGG = true;
        }
        else if (isHolding == true && heldObject.name == "Ladle")
        {
            isHoldingLADLE = true;
        }
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

        heldObject.transform.SetParent(grabPoint, worldPositionStays: false);
        heldObject.transform.localPosition = grabOffset;
        heldObject.transform.localRotation = Quaternion.Euler(grabRotationEuler);

        isHolding = true;     
    }


    public void ReleaseObject()
    {
        if (heldObject == null) return;
        {
            heldObject.transform.SetParent(null); // 解除抓取點
            heldObject.transform.position = originalPosition;
            heldObject.transform.rotation = originalRotation;
            //heldObject.transform.SetParent(originalParent); // 還原階層
        }
        if (heldRb != null)
        {
            heldRb.velocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.isKinematic = true; // ✅ 保持 kinematic，不掉落
        }
       

        heldObject = null;
        heldRb = null;
        isHolding = false; // <--- 加這行
    }
}