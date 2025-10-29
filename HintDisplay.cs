using UnityEngine;
using TMPro;
using System.Collections; // 必須引用 System.Collections 來使用協程

public class HintSequencer : MonoBehaviour
{
    [Header("UI 元件")]
    public TextMeshProUGUI hintTextComponent;
    public TextMeshProUGUI congratsTextComponent;

    [Header("提示文字內容")]
    public string firstMessage = "歡迎來到廚房模擬世界!";
    public string secondMessage = "請把手移動到物品上面。";

    // 固定顯示時間
    public float messageDuration = 5.0f; 

    // 用來控制顯示/隱藏的計時器 (保留在 Update 中使用，但建議改用協程處理計時)
    private float timer = 0f;
    private bool isDisplaying = false;

    // =========================================================
    // 遊戲啟動時自動開始序列
    // =========================================================
    void Start()
    {
        // 確保 Text(TMP) 初始狀態為隱藏
        if (hintTextComponent != null)
        {
            hintTextComponent.gameObject.SetActive(false);
        }
        
        // 啟動時間序列控制
        StartCoroutine(StartSequence());
    }

    // Update 邏輯 (可以保留用來處理 ShowHint 外部呼叫的持續計時，但序列本身不依賴它)
    void Update()
    {
        if (isDisplaying)
        {
            timer += Time.deltaTime;
            if (timer >= messageDuration)
            {
                // 如果是 Update 驅動的隱藏，則呼叫 HideHint
                // 但在序列中，我們依靠協程，所以這裡的邏輯可以忽略或用於其他動態計時
                // 為了程式碼簡潔，我們專注於協程
                // HideHint(); 
            }
        }
    }
    
    // =========================================================
    // 協程：控制文字方塊的顯示序列
    // =========================================================
    IEnumerator StartSequence()
    {
        // --- 第一個文字方塊 ---
        ShowHint(firstMessage, messageDuration);
        
        // 等待第一個文字方塊顯示完成的時間
        yield return new WaitForSeconds(messageDuration);

        // 隱藏第一個文字方塊
        HideHint(); 
        
        // (可選) 在兩個文字之間添加短暫的延遲，讓畫面更流暢
        yield return new WaitForSeconds(0.5f); 

        // --- 第二個文字方塊 ---
        ShowHint(secondMessage, messageDuration);
        
        // 等待第二個文字方塊顯示完成的時間
        yield return new WaitForSeconds(messageDuration);

        // 隱藏第二個文字方塊
        
        
        // 序列結束
    }


    // =========================================================
    // 顯示/隱藏函式 (與您原有的相同，但移除了 duration 的使用，因為計時改由協程控制)
    // =========================================================
    public void ShowHint(string message, float duration)
    {
        if (hintTextComponent == null)
        {
            Debug.LogError("HintTextComponent 未設定！請在 Inspector 中拖曳元件。");
            return;
        }

        hintTextComponent.text = message;
        hintTextComponent.gameObject.SetActive(true);

        // 協程控制了計時，所以我們只需要設置 displayDuration 供 Update 參考
        messageDuration = duration;
        timer = 0f;
        isDisplaying = true;
    }

    public void HideHint()
    {
        if (hintTextComponent != null)
        {
            hintTextComponent.gameObject.SetActive(false);
        }
        isDisplaying = false;
        timer = 0f;
    }
    public void ShowCongrats(string message, float duration)
    {
        if (congratsTextComponent == null)
        {
            Debug.LogError("CongratsTextComponent 未設定！無法顯示祝賀。");
            return;
        }

        // 停止可能正在執行的舊祝賀協程，避免閃爍或計時錯誤
        StopAllCoroutines();

        congratsTextComponent.text = message;
        congratsTextComponent.gameObject.SetActive(true);

        
    }
    IEnumerator AutoFadeCongrats(float duration)
    {
        // 等待指定的秒數
        yield return new WaitForSeconds(duration);
        
        // 隱藏祝賀訊息
        if (congratsTextComponent != null)
        {
            congratsTextComponent.gameObject.SetActive(false);
        }
    }
}