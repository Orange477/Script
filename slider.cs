using UnityEngine;
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
    
    if (v >= 0.0f   && v <= 0.135f)  result = "沒什麼味道...?";
    else if (v > 0.135f && v <= 0.343f)  result = "好像有點淡...";
    else if (v > 0.343f && v <= 0.66f)   result = "完美";
    else if (v > 0.66f  && v <= 0.87f)   result = "好像多了點...";
    else if (v > 0.87f  && v <= 1.0f)    result = "太多啦!!!";
    
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
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
                statusText.text = "運行中… (按空白鍵暫停)";
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