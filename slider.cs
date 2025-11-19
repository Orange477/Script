/*using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Scrollbar))]
public class AutoScrollbarController : MonoBehaviour
{
    public class ValueRangeText
    {
        public float min;           // 區間最小值（包含）
        public float max;           // 區間最大值（包含）
        public string text;         // 顯示文字
    }


    [Tooltip("從小到大排列，範圍必須覆蓋 0~1")]
    private string currentRangeText = "";
    private void UpdateRangeText()
    {
        float v = scrollbar.value;               // 當前值 0~1
        string result = "未知";

        if (v >= 0.0f && v <= 0.135f) result = "沒什麼味道...?";
        else if (v > 0.135f && v <= 0.343f) result = "好像有點淡...";
        else if (v > 0.343f && v <= 0.66f) result = "完美";
        else if (v > 0.66f && v <= 0.87f) result = "好像多了點...";
        else if (v > 0.87f && v <= 1.0f) result = "太多啦!!!";

        currentRangeText = result;

        // 顯示在 UI
        if (rangeDisplayText != null)
            rangeDisplayText.text = result;
    }

    [Header("自動移動設定")]
    public float moveSpeed = 0.7f;     // 每秒變化多少 (0~1)
    public bool pingPong = true;      // true = 來回, false = 單向循環

    [Header("UI 提示")]
    public Text statusText;        // 顯示「運行中 / 已暫停」
    public Text rangeDisplayText;   // 專門顯示區間文字

    private Scrollbar scrollbar;
    private Coroutine moveRoutine;
    private bool isMoving = true;

    void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    void Start()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();
        StartMoving();
        statusText.text = "運行中… ";

    }


    void Update()
    {
        // 空白鍵切換 播放 / 暫停
        if (Input.GetKeyDown(KeyCode.Space))  //FIXME:改成辨識手勢
        {
            TogglePause();
        }
    }*/
    
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Leap;
using Leap.Unity;

[RequireComponent(typeof(Scrollbar))]
public class AutoScrollbarController : MonoBehaviour
{
    // ====== 新增：Leap Provider 參考 ======
    [Header("Leap Motion")]
    public LeapServiceProvider leapProvider;

    // 你原本的欄位...
    public Text statusText;
    public Text rangeDisplayText;
    public float moveSpeed = 0.7f;
    public bool pingPong = true;

    public Canvas targetCanvas;
    public float autoCloseDelay = 2f;

    private Scrollbar scrollbar;
    private Coroutine moveRoutine;
    private bool isMoving = true;
    private bool hasFirstPaused = false;
    private bool isClosing = false;
    private string currentRangeText = "";

    void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    void Start()
    {
        if (leapProvider == null)
            leapProvider = FindObjectOfType<LeapServiceProvider>();

        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        StartMoving();

        if (statusText != null)
            statusText.text = "運行中… (握拳停止)";
    }

    void Update()
    {
        // 1. 先拿到目前的 Frame 與手
        if (leapProvider == null || leapProvider.CurrentFrame == null)
            return;

        Frame frame = leapProvider.CurrentFrame;
        if (frame.Hands.Count == 0)
            return;

        Hand hand = frame.Hands[0];   // 先用第一隻手

        // 2. 偵測「握拳」→ 呼叫 TogglePause()
        if (IsClosedFist(hand))
        {
            // 為了避免一握拳就每幀都觸發，可以加個簡單防抖：
            // 這裡用「只要狀態有變化才觸發」的邏輯，你也可以自己用 bool 紀錄前一幀狀態
            TogglePause();
        }
    }

    // ====== 手勢判斷：握拳 ======
    private bool IsClosedFist(Hand hand)
    {
        // 非拇指手指都要彎曲 (沒伸直)
        foreach (Finger finger in hand.Fingers)
        {
            if (finger.Type == Finger.FingerType.TYPE_THUMB)
                continue;

            if (finger.IsExtended)
                return false;
        }
        return true;
    }

    // ====== 下面都是你原本的程式（內容不變） ======

    private void UpdateRangeText()
    {
        float v = scrollbar.value;
        string result = "未知";

        if (v >= 0.0f && v <= 0.135f) result = "沒什麼味道...?";
        else if (v > 0.135f && v <= 0.343f) result = "好像有點淡...";
        else if (v > 0.343f && v <= 0.66f) result = "完美";
        else if (v > 0.66f && v <= 0.87f) result = "好像多了點...";
        else if (v > 0.87f && v <= 1.0f) result = "太多啦!!!";

        currentRangeText = result;

        if (rangeDisplayText != null)
            rangeDisplayText.text = result;
    }

    public void TogglePause()
    {
        isMoving = !isMoving;

        if (isMoving)
        {
            StartMoving();

            // 如果正在倒數關閉，取消它
            if (isClosing)
            {
                StopCoroutine("AutoCloseCanvas");
                isClosing = false;
            }

            if (statusText != null)
                statusText.text = "運行中… (按空白鍵停止)";
        }
        else
        {
            StopMoving();
            UpdateRangeText();  // 顯示當前區間

            if (statusText != null)
                statusText.text = $"{currentRangeText}";

            // === 第一次暫停：啟動 2 秒後關閉 Canvas ===
            if (!hasFirstPaused && targetCanvas != null)
            {
                hasFirstPaused = true;
                StartCoroutine(AutoCloseCanvas(autoCloseDelay));
            }
        }
    }

    [Header("自動關閉 Canvas")]
    public Canvas targetCanvas;
    public float autoCloseDelay = 2f;
    private bool hasFirstPaused = false;
    private bool isClosing = false;

    private IEnumerator AutoCloseCanvas(float delay)
    {
        isClosing = true;
        yield return new WaitForSeconds(delay);

        if (targetCanvas != null)
            targetCanvas.gameObject.SetActive(false);

        isClosing = false;
    }

    public void ResetAndStart()
    {
        StopAllCoroutines();
        scrollbar.value = 0f;
        isMoving = true;
        StartMoving();
    }

    private void StartMoving()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(AutoMoveScrollbar());
    }

    private void StopMoving()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private IEnumerator AutoMoveScrollbar()
    {
        float value = scrollbar.value;
        bool goingUp = true;

        while (true)
        {
            if (pingPong)
            {
                // 來回 (Ping-Pong)
                if (goingUp)
                {
                    value += moveSpeed * Time.deltaTime;
                    if (value >= 1f)
                    {
                        value = 1f;
                        goingUp = false;
                    }
                }
                else
                {
                    value -= moveSpeed * Time.deltaTime;
                    if (value <= 0f)
                    {
                        value = 0f;
                        goingUp = true;
                    }
                }
            }
            else
            {
                // 單向循環
                value += moveSpeed * Time.deltaTime;
                if (value > 1f)
                    value = 0f;   // 回到開頭
            }

            scrollbar.value = value;
            yield return null; // 每幀更新
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Play/Pause")]
    private void EditorToggle() => TogglePause();

    [ContextMenu("Reset & Start")]
    private void EditorReset() => ResetAndStart();
#endif
}




/*using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Leap;
using Leap.Unity;

[RequireComponent(typeof(Scrollbar))]
public class SeasoningMiniGame : MonoBehaviour
{
    [Header("=== 調味小遊戲設定 ===")]
    public float moveSpeed = 0.7f;
    public bool pingPong = true;
    public Canvas seasoningCanvas;     // 自動顯示/隱藏
    public Text statusText;
    public Text rangeDisplayText;
    public float autoCloseDelay = 2f;

    [Header("=== Leap Motion ===")]
    public LeapServiceProvider leapProvider;

    // 狀態
    private Scrollbar scrollbar;
    private Coroutine moveRoutine;
    private bool isActive = false;           // 是否處於調味步驟
    private bool hasPaused = false;          // 是否已第一次暫停
    private bool isHandGestureEnabled = true; // 手勢偵測開關（握拳後關閉）
    private string currentRangeText = "";

    // 事件：通知主遊戲完成
    public System.Action<int> OnSeasoningComplete;

    void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
        if (seasoningCanvas) 
            seasoningCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        if (leapProvider == null)
            leapProvider = FindObjectOfType<LeapServiceProvider>();
        
        if (leapProvider == null)
            Debug.LogError("SeasoningMiniGame: 找不到 LeapServiceProvider！");
    }

    // ==============================================================
    // 外部呼叫：進入調味步驟（主遊戲呼叫此方法）
    // ==============================================================
    public void StartSeasoning()
    {
        if (isActive) return;

        isActive = true;
        hasPaused = false;
        isHandGestureEnabled = true;  // 確保手勢啟用
        seasoningCanvas.gameObject.SetActive(true);
        scrollbar.value = 0f;
        moveRoutine = null;

        if (statusText) statusText.text = "請張開手掌開始調味…";
        Debug.Log("調味步驟開始！");
    }

    // ==============================================================
    // 外部呼叫：強制結束（例如跳過）
    // ==============================================================
    public void ForceEndSeasoning()
    {
        EndMiniGame();
    }

    // ==============================================================
    // Unity Update - 手勢偵測核心
    // ==============================================================
    void Update()
    {
        // 只有在小遊戲啟動 + 手勢啟用時才處理
        //if (!isActive || !isHandGestureEnabled) 
        //    return;
        if (!isActive)
            return;

        if (leapProvider == null || leapProvider.CurrentFrame == null) 
            return;

        Frame frame = leapProvider.CurrentFrame;
        if (frame.Hands.Count == 0) 
            return;

        Hand hand = frame.Hands[0]; // 只處理第一隻手

        // 張開手掌 → 開始移動
        if (IsOpenPalm(hand) && moveRoutine == null)
        {
            ResumeMoving();
        }
        // 握拳 → 停止 + 關閉手勢
        else if (IsClosedFist(hand) && moveRoutine != null)
        {
            PauseAndPrepareEnd();
        }
    }

    // ==============================================================
    // 手勢判斷
    // ==============================================================
    private bool IsOpenPalm(Hand hand)
    {
        int extendedCount = 0;
        foreach (Finger finger in hand.Fingers)
        {
            if (finger.IsExtended) 
                extendedCount++;
        }
        return extendedCount >= 4; // 至少4指伸直
    }

    private bool IsClosedFist(Hand hand)
    {
        // 非拇指手指都要彎曲
        foreach (Finger finger in hand.Fingers)
        {
            if (finger.Type == Finger.FingerType.TYPE_THUMB) 
                continue;
            if (finger.IsExtended) 
                return false;
        }
        return true;
    }

    // ==============================================================
    // 開始移動
    // ==============================================================
    private void ResumeMoving()
    {
        if (moveRoutine != null) return;
        
        moveRoutine = StartCoroutine(AutoMoveScrollbar());
        if (statusText) 
            statusText.text = "調味中… (握拳停止)";
    }

    // ==============================================================
    // 握拳：停止移動 + 立即關閉手勢 + 開始倒數
    // ==============================================================
    private void PauseAndPrepareEnd()
    {
        // 1. 停止移動
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        // 2. 更新顯示
        UpdateRangeText();
        if (statusText) 
            statusText.text = $"{currentRangeText} (2秒後結束)";

        // 3. 立即關閉手勢偵測（避免誤觸）
        //isHandGestureEnabled = false;

        // 4. 第一次暫停：啟動自動結束
        if (!hasPaused)
        {
            hasPaused = true;
            StartCoroutine(AutoEndAfterDelay());
        }
    }

    // ==============================================================
    // 2秒後自動結束小遊戲
    // ==============================================================
    private IEnumerator AutoEndAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        EndMiniGame();
    }

    // ==============================================================
    // 結束小遊戲（關閉UI + 通知主遊戲）
    // ==============================================================
    private void EndMiniGame()
    {
        if (!isActive) return;

        // 通知主遊戲調味結果
        int level = GetSeasoningLevel();
        OnSeasoningComplete?.Invoke(level);

        // 清理
        StopAllCoroutines();
        if (moveRoutine != null) 
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        if (seasoningCanvas) 
            seasoningCanvas.gameObject.SetActive(false);

        // 重置狀態（準備下次使用）
        isActive = false;
        hasPaused = false;
        isHandGestureEnabled = true;

        Debug.Log($"調味小遊戲結束！最終程度：{level} ({currentRangeText})");
    }

    // ==============================================================
    // 區間文字與等級判斷
    // ==============================================================
    private void UpdateRangeText()
    {
        float v = scrollbar.value;
        string result = "未知";

        if (v <= 0.135f)       result = "沒什麼味道...?";
        else if (v <= 0.343f)  result = "好像有點淡...";
        else if (v <= 0.66f)   result = "完美";
        else if (v <= 0.87f)   result = "好像多了點...";
        else                   result = "太多啦!!!";

        currentRangeText = result;
        if (rangeDisplayText) 
            rangeDisplayText.text = result;
    }

    private int GetSeasoningLevel()
    {
        float v = scrollbar.value;
        if (v <= 0.135f) return 0;
        else if (v <= 0.343f) return 1;
        else if (v <= 0.66f) return 2;
        else if (v <= 0.87f) return 3;
        else return 4;
    }

    // ==============================================================
    // Scrollbar 自動移動協程
    // ==============================================================
    private IEnumerator AutoMoveScrollbar()
    {
        float value = scrollbar.value;
        bool goingUp = true;

        while (true)
        {
            if (pingPong)
            {
                // 來回移動
                if (goingUp)
                {
                    value += moveSpeed * Time.deltaTime;
                    if (value >= 1f)
                    {
                        value = 1f;
                        goingUp = false;
                    }
                }
                else
                {
                    value -= moveSpeed * Time.deltaTime;
                    if (value <= 0f)
                    {
                        value = 0f;
                        goingUp = true;
                    }
                }
            }
            else
            {
                // 單向循環
                value += moveSpeed * Time.deltaTime;
                if (value > 1f) 
                    value = 0f;
            }

            scrollbar.value = value;
            UpdateRangeText();
            yield return null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("測試：開始調味")]
    private void EditorTestStart() => StartSeasoning();

    [ContextMenu("測試：強制結束")]
    private void EditorTestEnd() => ForceEndSeasoning();
#endif
}*/