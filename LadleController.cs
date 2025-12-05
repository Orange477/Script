using UnityEngine;
using System.Collections; // 需要用到 Coroutine
using UnityEngine.Video; // 引入 Video 命名空間
using UnityEngine.UI; // 為了 RawImage

// 假設這個腳本掛載在湯勺 (Ladle) 物件上
public class LadleController : MonoBehaviour
{
    [Header("目標設定")]
    public string panTag = "pan"; // 鍋子的 Tag
    [Header("影片播放設定")]
    public VideoPlayer cookingVideoPlayer; // 新增 VideoPlayer 引用
    public RawImage videoDisplayImage;
    private bool isWaitingForVideoEnd = false;

    [Header("流程控制")]
    [Tooltip("請將 ItemPrompt 腳本所在的遊戲物件拖曳到此處。")]
    public ItemPrompt itemPromptManager; // 這是 Class Field
    [Header("翻面物件參考")]
    [Tooltip("請將打入鍋中'實際存在的蛋液物件'拖曳到此處。")]
    public Transform eggToFlip; // 這次我們直接控制蛋的 Transform

    [Header("翻面參數")]
    public float flipDuration = 0.5f; // 翻轉 360 度的時間（秒）
    public Vector3 flipAxis = Vector3.forward; // 翻轉的軸向 (例如：z 軸)

    public bool isFlipping = false; // 防止重複翻面
    public bool flipCompleted = false; // 標記翻面是否完成
    public HintSequencer hintSequencer;
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        // 1. 隱藏顯示影片的 Raw Image，畫面回到 3D 場景
        if (videoDisplayImage != null)
        {
            videoDisplayImage.gameObject.SetActive(false);
        }
        
        // 2. [重要] 解除訂閱事件，避免下次播放時重複執行
        vp.loopPointReached -= OnVideoFinished;
        
        // 3. 通知協程，等待結束
        isWaitingForVideoEnd = false; 
        
        Debug.Log("影片播放結束 by Event.");
    }
    // 湯勺的 Collider 應該設為 Is Trigger
   private void OnTriggerStay(Collider other) // 這是新的，每一幀都會執行
    {
        // 【測試 Log A】這條 Log 應該在湯勺停留時持續出現
        Debug.Log($"Ladle Staying! Name: {other.gameObject.name}, Tag: {other.gameObject.tag}"); 

        // 1. 檢查是否觸碰到鍋子
        if (!other.gameObject.CompareTag(panTag))
        {
            return; // 如果不是鍋子，則直接退出
        }

        // 2. 【流程檢查】如果未啟用翻面，則退出
        if (itemPromptManager == null || !itemPromptManager.canStartFlipping)
        {
            // 流程尚未啟用，給予 Log 提示並退出
            Debug.Log("Ladle Staying: 翻面功能尚未啟用 (調味未完成)。");
            return; 
        }

        // --- 流程檢查通過 (調味已完成) ---

        // 3. 檢查蛋的參考和翻面狀態
        // isFlipping 旗標會確保 PerformFlip 協程只啟動一次
        if (eggToFlip != null && !isFlipping && !flipCompleted)
        {
            Debug.Log("!!! SUCCESS: ALL CHECKS PASSED, STARTING FLIP !!!");
            
            // 啟動翻面協程
            StartCoroutine(PerformFlip(eggToFlip, flipDuration, flipAxis));
        }
        else
        {
            // 在翻面完成後，這條 Log 會持續出現，因為 isFlipping=True
            Debug.Log("Ladle Staying: 蛋已翻面或參考遺失，等待動畫結束或手移開。");
            flipCompleted = true;
        }
    }
    
    /// <summary>
    /// 執行 360 度平滑翻轉的協程。
    /// </summary>
    private IEnumerator PerformFlip(Transform targetEgg, float duration, Vector3 axis)
    {
        isFlipping = true; // 設置旗標

        float startTime = Time.time;
        float elapsed = 0f;
        Quaternion startRotation = targetEgg.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(axis * 360f);

        while (elapsed < duration)
        {
            elapsed = Time.time - startTime;
            float t = elapsed / duration; // 0 到 1 的插值比率
            
            // 使用 Lerp 平滑旋轉
            targetEgg.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null; // 等待下一幀
        }

        // 確保精確到達目標旋轉 (360度即回到原位)
        targetEgg.localRotation = targetRotation; 
        Debug.Log("蛋翻面完成。");
        if (hintSequencer != null)
        {
            hintSequencer.ShowHint("翻面完成!!", 3.0f);
        }

        yield return new WaitForSeconds(5.0f);

        if (hintSequencer != null)
        {
            hintSequencer.ShowHint("準備起鍋~", 3.0f);
        }

        yield return new WaitForSeconds(5.0f);

        // *******************************************************************
        // 🚨 關鍵修改：播放影片並等待 🚨
        if (cookingVideoPlayer != null && cookingVideoPlayer.clip != null)
        {
            Debug.Log("開始播放影片...");
            
            // 1. 設置等待旗標為 True
            isWaitingForVideoEnd = true; 
            
            // 2. [重要] 訂閱事件：當影片到達結尾時，呼叫 OnVideoFinished 方法
            cookingVideoPlayer.loopPointReached += OnVideoFinished; 
            
            // 3. 啟用顯示影片的 UI Image 
            if (videoDisplayImage != null)
            {
                videoDisplayImage.gameObject.SetActive(true); 
            }
            
            // 4. 播放影片
            cookingVideoPlayer.Play();

            // 5. 暫停協程：等待 OnVideoFinished 將 isWaitingForVideoEnd 設為 false
            yield return new WaitUntil(() => isWaitingForVideoEnd == false);
            
            // 此處協程繼續執行，表示影片已播完且 Raw Image 已被隱藏
        }
        else
        {
            Debug.LogError("未找到 Video Player 或 Clip，跳過影片播放步驟。");
        }
        // *******************************************************************

        flipCompleted = true; // 標記翻面完成
        FindObjectOfType<GameTimer>().StopTimer();
        Debug.Log("所有流程結束。");
        // 標記翻面完成   
        

        // 【可選】如果翻面只需要觸發一次，可以在這裡禁用湯勺的 Trigger
        // GetComponent<Collider>().enabled = false; 
    }
}
