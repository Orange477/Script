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
        [HideInInspector] public bool isActive;
    }

    [Tooltip("從小到大排列，範圍必須覆蓋 0~1")]
    public ValueRangeText[] rangeTexts = new ValueRangeText[]
    {
        new ValueRangeText { min = 0.0f, max = 0.135f, text = "低" },
        new ValueRangeText { min = 0.135f, max = 0.343f, text = "中" },
        new ValueRangeText { min = 0.343f, max = 0.66f, text = "完美" },
        new ValueRangeText { min = 0.87f, max = 1.0f, text = "低" },
        new ValueRangeText { min = 0.66f, max = 0.87f, text = "中" }
    };

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
        StartMoving();
        UpdateStatusText();
    }

    void Update()
    {
        // 空白鍵切換 播放 / 暫停
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }

    // ──────────────────────────────────────
    // 公開方法：外部呼叫（例如按鈕）
    // ──────────────────────────────────────
    public void TogglePause()
    {
        isMoving = !isMoving;

        if (isMoving)
            StartMoving();
        else
            StopMoving();

        UpdateStatusText();
        Debug.Log(isMoving ? "Scrollbar 繼續移動" : "Scrollbar 已暫停");
    }

    public void ResetAndStart()
    {
        StopAllCoroutines();
        scrollbar.value = 0f;
        isMoving = true;
        StartMoving();
        UpdateStatusText();
        rangeDisplayText();
    }

    // ──────────────────────────────────────
    // 內部協程
    // ──────────────────────────────────────
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

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isMoving
                ? "運行中… (按空白鍵暫停)"
                : "已暫停 (按空白鍵繼續)";
        }
    }

    // ──────────────────────────────────────
    // 編輯器小工具：Play 模式下直接測試
    // ──────────────────────────────────────
#if UNITY_EDITOR
    [ContextMenu("Play/Pause")]
    private void EditorToggle() => TogglePause();

    [ContextMenu("Reset & Start")]
    private void EditorReset() => ResetAndStart();
#endif
}