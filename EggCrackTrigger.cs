using UnityEngine;
using Leap.Unity.Interaction; // 假設您使用 Leap Motion

// 假設這個腳本是掛在 "蛋殼" 物件上
public class EggCrackTrigger : MonoBehaviour
{
    // 【新增】流程管理器參考
    [Header("流程管理器")]
    public ItemPrompt itemPromptManager; // 將 ItemPrompt 腳本所在的物件拖曳到此欄位

    [Header("打蛋後的設定")]
    public GameObject uncookedEggPrefab; // 打開後流出的蛋液 Prefab
    public Transform panTransform;       // 鍋子的位置 
    
    [Header("移動速度閾值 (絕對值)")]
    public float movementThreshold = 3.0f; 

    private bool isCracked = false;
    private Vector3 lastPos; 
    private float lastTime;  

    void Start()
    {
        lastPos = transform.position;
        lastTime = Time.time;
    }

    void Update()
    {
        if (isCracked) return;

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
        Vector3 currentPos = transform.position;
        float currentTime = Time.time;

        if (lastTime > 0f)
        {
            float deltaTime = currentTime - lastTime;
            if (deltaTime > 0)
            {
                float velocityY = (currentPos.y - lastPos.y) / deltaTime;
                
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
        
        // 【修正點 1】: 將 spawnedEgg 宣告到方法頂部
        GameObject spawnedEgg = null; 

        // 1. 生成打好的蛋液
        if (uncookedEggPrefab != null && panTransform != null)
        {
            spawnedEgg = Instantiate(uncookedEggPrefab, panTransform.position, Quaternion.identity);
        }
        
        // 2. 銷毀蛋殼
        // 不需要遷移 ItemPrompt，因為它已經在 Ladle 上了。
        Destroy(gameObject); 
        Debug.Log("蛋已打入鍋中，蛋殼銷毀！");

        // 3. 找到湯勺控制器並賦予蛋的參考 (LadleController 和 ItemPrompt 在同一個物件上)
        // 我們需要確保 LadleController 能夠被找到
        LadleController ladleController = FindObjectOfType<LadleController>();

        if (ladleController != null && spawnedEgg != null)
        {
            // 傳遞生成的蛋液物件的 Transform 給 LadleController
            ladleController.eggToFlip = spawnedEgg.transform;
            Debug.Log("已將新蛋液的參考傳遞給湯勺控制器。");
        }
        else
        {
            Debug.LogError("LadleController 或生成的蛋液遺失，無法傳遞翻面參考。");
        }

        // 4. 觸發 Ladle 上的 ItemPrompt 流程
        if (itemPromptManager != null)
        {
            // itemPromptManager 現在指向 Ladle 上的 ItemPrompt.cs
            itemPromptManager.OnEggCracked(); 
            Debug.Log("已呼叫 ItemPrompt.OnEggCracked() 啟動調味流程。");
        }
        else
        {
            Debug.LogError("ItemPrompt Manager (Ladle) 參考遺失，無法啟動流程。");
        }
    }
}
