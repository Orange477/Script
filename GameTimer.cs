using UnityEngine;
using TMPro; // 引入 TextMeshPro 的命名空間

public class GameTimer : MonoBehaviour
{
    // 變數類型改為 TextMeshProUGUI
    public TMPro.TextMeshProUGUI timerText; 
    
    private float elapsedTime = 0f;
    private bool isRunning = false;

    void Start()
    {
        StartTimer(); 
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        UpdateTimerUI(); 
    }

    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("遊戲總時間：" + elapsedTime.ToString("F2"));
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI(); 
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 透過 .text 屬性更新 TextMeshPro 的內容
            timerText.text = "時間：" + elapsedTime.ToString("F2");
        }
    }
}
