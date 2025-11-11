using UnityEngine;
using Leap.Unity.Interaction;

// 假設這個腳本是掛在 "蛋殼" 物件上
public class EggCrackTrigger : MonoBehaviour
{
    // 不再需要手部相關的 public 變數，因為我們判斷的是自身的位移
    // [Header("手部與手模型")]
    // public InteractionHand hand;
    // public Transform handTransform; // 不再需要，直接使用 transform

    [Header("打蛋後的設定")]
    public GameObject uncookedEggPrefab; // 打開後流出的蛋液 Prefab
    public Transform panTransform;       // 鍋子的位置 (通常在遊戲開始時由抓取管理器或環境設定)
    
    [Header("移動速度閾值 (絕對值)")]
    // 當 Y 軸速度的絕對值超過此值時，觸發打蛋。
    // 數值越小越靈敏。建議值：1.0f 到 5.0f。
    public float movementThreshold = 3.0f; 

    private bool isCracked = false;
    private Vector3 lastPos; // 儲存上次自身的位置
    private float lastTime;  // 儲存上次的時間

    // 注意：isEggInHand 和 grabPoint 不再需要，因為如果腳本在蛋上，就代表它現在是蛋

    void Start()
    {
        // 初始化位置和時間
        lastPos = transform.position;
        lastTime = Time.time;

        // 【重要】如果 panTransform 是由另一個物件提供的，請確保它被正確賦值。
        // 如果您希望這個腳本能自動找到鍋子，請使用以下方法：
        // if (panTransform == null)
        // {
        //     GameObject pan = GameObject.FindWithTag("Pan");
        //     if (pan != null)
        //     {
        //         panTransform = pan.transform;
        //     }
        // }
    }

    void Update()
    {
        if (isCracked)
        {
            // Debug.Log("蛋已打出");
            return;
        }

        // 偵測蛋是否有顯著移動
        if (DetectAnySignificantMovement())
        {
            CrackEgg();
        }

        // 更新上次的位置和時間
        lastPos = transform.position;
        lastTime = Time.time;
    }

    /// <summary>
    /// 偵測蛋 (自身) 的 Y 軸速度絕對值是否超過閾值
    /// </summary>
    bool DetectAnySignificantMovement()
    {
        // Debug.Log("打蛋偵測執行中 (偵測自身移動)");
        Vector3 currentPos = transform.position;
        float currentTime = Time.time;

        if (lastTime > 0f)
        {
            float deltaTime = currentTime - lastTime;
            if (deltaTime > 0)
            {
                // 計算 Y 軸速度
                float velocityY = (currentPos.y - lastPos.y) / deltaTime;
                
                // Debug.Log($"velocityY: {velocityY}, Absolute: {Mathf.Abs(velocityY)}");

                // 檢查速度的**絕對值**是否超過閾值 (向上或向下快速移動都算)
                if (Mathf.Abs(velocityY) > movementThreshold)
                { 
                    Debug.Log($"觸發移動！速度: {velocityY}");
                    return true;
                }
            }
        }
        
        return false;
    }

    void CrackEgg()
    {
        Debug.Log("開打");
        if (isCracked) return;
        isCracked = true;
        
        // 1. 生成打好的蛋液
        if (uncookedEggPrefab != null && panTransform != null)
        {
            // 在鍋子的位置生成打好的蛋液
            Instantiate(uncookedEggPrefab, panTransform.position, Quaternion.identity);
        }
        
        // 2. 銷毀或隱藏自身 (蛋殼)
        // 如果這個腳本是掛在蛋殼上，銷毀它即可
        Destroy(gameObject); 

        Debug.Log("蛋已打入鍋中，蛋殼銷毀！");
    }
}