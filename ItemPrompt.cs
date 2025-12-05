using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrompt : MonoBehaviour
{
    [Header("UI 控制器")]
    public HintSequencer hintSequencer;

    [Header("互動流程控制")]
    public bool canStartFlipping = false; // 預設為 False

    // 【修正】將 slider 替換為正確的類別名稱 AutoScrollbarController
    [Header("調味滑軌控制器")]
    public AutoScrollbarController flavoringUI;

    [Header("接觸設定")]
    public string handTag = "Hand"; // 你的手部碰撞體的 Tag
    public float requiredTouchTime = 5.0f; // 必須持續碰觸的秒數

    // 內部計時器變數
    private float touchTimer = 0f;
    private bool handIsTouching = false;
    private bool promptIsActive = false; // 追蹤提示是否正在顯示
    private bool Isfivefinished = false; // 標記 5 秒接觸是否完成
    private bool isResetting = false; // 【防抖邏輯】標記是否正在重置計時器
    private bool isResettingfinishing = false;
    private string finalresult;
    private bool isPeppershowed = false;
    
    private bool isFlavoringComplete = false;
    public LadleController ladleController;
    

    // Start is called before the first frame update
    void Start()
    {
        // 確保滑軌的 Canvas 預設是關閉的
        if (flavoringUI != null && flavoringUI.targetCanvas != null)
        {
             flavoringUI.targetCanvas.gameObject.SetActive(false);
        }
        FindObjectOfType<GameTimer>().StartTimer();
    }

    // Update is called once per frame
    void Update()
    {   
        Debug.Log($"isResetting 狀態：{isResetting}");
        if (isResetting)
        {
            Debug.Log("計時器重製");
        }
        if (handIsTouching)
        {
            touchTimer += Time.deltaTime;
            Debug.Log("持續接觸時間: " + touchTimer.ToString("F2") + " 秒");
            if (isResetting)
            {
                isResettingfinishing = true;
                Debug.Log("計時器重製完成");
            }
            isResetting = false;
            if (touchTimer >= requiredTouchTime)
            {
                // **流程完成：阻止計時繼續**
                handIsTouching = false;
                touchTimer = 0f;

                Debug.Log("✔ 成功持續接觸 " + requiredTouchTime + " 秒！");

                // 隱藏初次提示
                if (promptIsActive)
                {
                    hintSequencer.HideHint();
                    promptIsActive = false;
                    Isfivefinished = true; // 標記 5 秒任務完成
                }

                // 顯示下一個步驟提示 (打蛋)
                if (Isfivefinished)
                {
                    hintSequencer.ShowHint("接下來請試著打蛋", 3.0f);
                    // 注意：這裡不設 Isfivefinished = false，它標記了流程狀態
                }
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ItemPrompt OnTriggerEnter 被觸發");
        Debug.Log($"Enter Trigger事件: {other.name}");
        Debug.Log($"OnTriggerEnter ({other.name})，座標：{other.transform.position}");
        if (other.CompareTag(handTag))
        {

            Debug.Log("手進入碰撞區域，開始計時");
            isResetting = false;
                
                // 顯示提示文字
            if (!promptIsActive)
            {
                    hintSequencer.ShowHint("請先將手保持在畫面上五秒。", 9999f);
                    touchTimer = 0f; 
                    promptIsActive = true;
            }
                // 開始接觸計時
            handIsTouching = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("ItemPrompt OnTriggerExit 被觸發");
        Debug.Log($"Exit Trigger事件: {other.name}");
        if (!other.CompareTag(handTag))
        {

            // 重置狀態和計時器
            handIsTouching = false;
            touchTimer = 0f;

            // 消除提示
            if (promptIsActive)
            {
                hintSequencer.HideHint();
                promptIsActive = false;
            }
            isResetting = true;
            Debug.Log("手部完全離開，計時被中斷並已重置。");
        }
    }

    // ──────────────────────────────────────
    // 打蛋完成後流程控制 (由 EggCrackTrigger 呼叫)
    // ──────────────────────────────────────
    public void OnEggCracked()
    {
        Debug.Log("ItemPrompt 收到打蛋完成通知，開始後續流程。");
        StopAllCoroutines(); 
        StartCoroutine(PostCrackSequence());
    }

    private IEnumerator PostCrackSequence()
    {
        // 1. 顯示「非常好!」 (停留 3 秒)
        if (hintSequencer != null)
        {
            hintSequencer.ShowHint("非常好!", 3.0f);
        }
        
        yield return new WaitForSeconds(8.0f); 

        // 2. 顯示「接著來模擬調味!」 (停留 3 秒)
        if (hintSequencer != null)
        {
            hintSequencer.ShowHint("接著來撒鹽巴!", 3.0f);
        }

        yield return new WaitForSeconds(3.0f); 


        // 3. 呼叫 slider.cs (AutoScrollbarController)
        Debug.Log("啟動調味滑軌！");
        if (flavoringUI != null)
        {
            isFlavoringComplete = false; // 重置旗標
            // 啟用滑軌所在的 Canvas
            if (flavoringUI.targetCanvas != null)
            {
                flavoringUI.targetCanvas.gameObject.SetActive(true);
                hintSequencer.HideHint(); 
            }

            flavoringUI.OnFlavoringFinished = () =>
            {
                Debug.Log("鹽巴調味結束，觸發結果提示！");
                finalresult = hintSequencer.GetFinalDishCommentSalt();
                
                // 顯示結果提示 (3 秒)
                
                hintSequencer.ShowHint(finalresult, 3.0f); // 防止自動關閉邏輯干擾提示顯示
                
                StartCoroutine(DelayFlavoringComplete(3.0f)); 
            };

            // 確保滑軌開始移動
            flavoringUI.ResetAndStart(); 

            
            
            // 【修正】完成流程後，將 Isfivefinished 設回 false，避免流程重複觸發打蛋後的提示
            Isfivefinished = false; 
        }
        else
        {
            Debug.LogError("ItemPrompt 的 flavoringUI 欄位尚未指定！無法啟動調味滑軌。請在 Unity Inspector 中拖曳正確的物件。");
            isFlavoringComplete = true; // 避免卡死，直接設為完成
        }

        yield return new WaitUntil(() => isFlavoringComplete);
        // 【重要】鹽巴流程結束，禁用滑軌 UI，以防影響下一個流程
        if (flavoringUI != null && flavoringUI.targetCanvas != null)
        {
            flavoringUI.targetCanvas.gameObject.SetActive(false);
        }
        hintSequencer.HideHint(); // 確保結果提示隱藏

        // 撒胡椒粉
        if (hintSequencer != null )
        {
            Debug.Log("接著來撒胡椒粉!");
            hintSequencer.ShowHint("接著來撒胡椒粉!", 3.0f);
        }

        yield return new WaitForSeconds(3.0f);
        isPeppershowed = true;

        Debug.Log("啟動調味滑軌！");
        if (flavoringUI != null && isPeppershowed) 
        {
            isFlavoringComplete = false; // 重置旗標

            // 啟用滑軌所在的 Canvas
            if (flavoringUI.targetCanvas != null)
            {
                flavoringUI.rangeDisplayText.text = "請撒胡椒粉!";
                flavoringUI.targetCanvas.gameObject.SetActive(true);
                hintSequencer.HideHint(); 
            }

            // 設置撒胡椒完成後的回呼
            flavoringUI.OnFlavoringFinished = () =>
            {
                Debug.Log("胡椒調味結束，觸發結果提示！");

                finalresult = hintSequencer.GetFinalDishCommentPepper();
                
                // 顯示結果提示 (3 秒)
                hintSequencer.ShowHint(finalresult, 3.0f); // 防止自動關閉邏輯干擾提示顯示
                
                
                // 【重要】在提示顯示完 3 秒後，才將旗標設為 true
                StartCoroutine(DelayFlavoringComplete(3.0f));
            };

            // 確保滑軌開始移動
            flavoringUI.ResetAndStart(); 
        }
        else
        {
            Debug.LogError("ItemPrompt 的 flavoringUI 欄位尚未指定！無法啟動調味滑軌。請在 Unity Inspector 中拖曳正確的物件。");
        }

        // 【新增】等待胡椒調味流程完全結束 (包括 3 秒結果提示)
        yield return new WaitUntil(() => isFlavoringComplete);
        
        // 【收尾】流程完全結束，禁用滑軌 UI
        if (flavoringUI != null && flavoringUI.targetCanvas != null)
        {
            flavoringUI.targetCanvas.gameObject.SetActive(false);
        }
        hintSequencer.HideHint(); 
        // 【重要新增】在所有調味流程結束後，啟用翻面功能
        canStartFlipping = true; 
        if(hintSequencer != null )
        {
            hintSequencer.ShowHint("調味完成!", 3.0f);
        }
        Debug.Log("所有調味流程結束。**翻面功能已啟用**。");
        yield return new WaitForSeconds(6.0f); 
        hintSequencer.ShowHint("現在請用湯勺試著將蛋翻面吧！", 3.0f);
    }
    // 幫助延遲設置旗標的協程
    private IEnumerator DelayFlavoringComplete(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        isFlavoringComplete = true;
    }
}
