using UnityEngine;
using Leap.Unity.Interaction;
using Leap;
using Leap.Unity; // 為了使用 ToVector3


public class GrabTrigger : MonoBehaviour
{
    [Header("手部與手模型")]
    public InteractionHand hand; // 指定 InteractionHand
    public Transform grabPoint;  // 抓取位置（最好是手掌上的空物件）
    

    [Header("抓取設定")]
    public string[] grabTags = new string[] { "Ladle", "egg" };
    public float gripThreshold = 0.8f; // 握拳判定閾值
    public float grabRange = 0.4f; // OnTriggerStay 判斷抓取範圍
    public Vector3 grabOffset = Vector3.zero; // 抓取時的本地位移
    public Vector3 grabRotationEuler = Vector3.zero; // 抓取時的本地旋轉

    public GameObject heldObject;
    private Rigidbody heldRb = null;
    private Transform originalParent;
    private Vector3 originalScale; // 新增：紀錄原始縮放

    public bool isHolding = false;
    public bool isHoldingEGG = false;
    public bool isHoldingLADLE = false;
    private float releaseCooldown = 0.2f;
    private float releaseTimer = 0f;

    void Start()
    {
        
        // 確保 grabPoint 已設定
        if (grabPoint == null)
        {
            Debug.LogError("GrabPoint 未設定！請將一個子物件拖曳進 Inspector。");
        }
        
        // 確保 InteractionBehaviour 存在且停用自動抓取
        InteractionBehaviour ib = GetComponent<InteractionBehaviour>();
        if (ib != null)
        {
            ib.throwHandler = null;       // 停用丟擲邏輯
            ib.ignoreGrasping = true;     // 停用 SDK 的自動抓取
        }
    }


    public void Update()
    {
        if (hand == null || hand.leapHand == null) return;

        Hand leapHand = hand.leapHand;
        float gripStrength = leapHand.GrabStrength;

        if (isHolding)
        {
            // --- 釋放邏輯 (在抓取狀態下才檢查) ---
            if (gripStrength < gripThreshold)
            {
                releaseTimer += Time.deltaTime;
                if (releaseTimer >= releaseCooldown)
                {
                    ReleaseObject();
                    releaseTimer = 0f;
                }
            }
            else
            {
                // 持續握拳，重置計時器
                releaseTimer = 0f;
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (isHolding) return; // 已經抓著物件，跳過

        // 檢查握拳強度，是否達到抓取閾值
        if (hand == null || hand.leapHand == null || hand.leapHand.GrabStrength < gripThreshold) return;


        GameObject target = null;
        Transform current = other.transform;

        // 往上檢查父層級的 Tag
        while (current != null)
        {
            foreach (var tag in grabTags)
            {
                // 檢查 Tag 或名稱是否符合 (為了更靈活)
                if (current.CompareTag(tag) || current.name.ToLower().Contains(tag.ToLower()))
                {
                    target = current.gameObject;
                    break;
                }
            }
            if (target != null)
            {
                // 找到了
                break;
            }
            current = current.parent;
        }

        if (target != null)
        {
            GrabObject(target);
        }
    }


    public void GrabObject(GameObject obj)
    {
        heldObject = obj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        // 1. 紀錄原始狀態 (Parent 和 Scale)
        originalParent = obj.transform.parent;
        originalScale = obj.transform.localScale;

        if (heldRb != null)
        {
            heldRb.velocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.isKinematic = true; // 禁用物理，穩定吸附
        }
        
        // 2. 設置父物件、位置和旋轉
        // worldPositionStays: false 意即使用本地座標
        heldObject.transform.SetParent(grabPoint, worldPositionStays: false);
        heldObject.transform.localPosition = grabOffset;
        heldObject.transform.localRotation = Quaternion.Euler(grabRotationEuler);

        // 3. 確保縮放不會因為父物件縮放而改變
        heldObject.transform.localScale = Vector3.one; 

        // 4. 更新狀態
        isHolding = true;
        if (heldObject.name.ToLower().Contains("egg"))
        {
            isHoldingEGG = true;
            
        }
        else if (heldObject.name.ToLower().Contains("ladle"))
        {
            isHoldingLADLE = true;
            
        }
        Debug.Log("成功抓取物件：" + obj.name);
    }


    public void ReleaseObject()
    {
        if (heldObject == null) return;

        // 1. 還原父物件
        heldObject.transform.SetParent(originalParent, worldPositionStays: true);

        // 2. 還原縮放
        heldObject.transform.localScale = originalScale;

        if (heldRb != null)
        {
            // 3. 恢復物理 (讓它掉落)
            heldRb.isKinematic = false;

            // 4. 重設速度 (可選，避免殘留的速度讓物件亂飛)
            heldRb.velocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        // 5. 重設狀態
        if (heldObject.name.ToLower().Contains("egg")) isHoldingEGG = false;
        else if (heldObject.name.ToLower().Contains("ladle")) isHoldingLADLE = false;

        heldObject = null;
        heldRb = null;
        isHolding = false;
        Debug.Log("釋放物件");
    }
}