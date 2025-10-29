using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class ItemPrompt : MonoBehaviour

{

    [Header("UI 控制器")]

    // 將 HintSequencer 腳本所在的物件 (例如 UIManager) 拖曳到此欄位

    public HintSequencer hintSequencer;



    [Header("接觸設定")]

    public string handTag = "Hand"; // 你的手部碰撞體的 Tag

    public float requiredTouchTime = 5.0f; // 必須持續碰觸的秒數



    // 內部計時器變數

    private float touchTimer = 0f;

    private bool handIsTouching = false;

    private bool promptIsActive = false; // 追蹤提示是否正在顯示

    // Start is called before the first frame update

    void Start()

    {

       

    }



    // Update is called once per frame

    void Update()

    {

        if (handIsTouching)

        {

        touchTimer += Time.deltaTime;
            if (touchTimer >= requiredTouchTime)

            {

                if (promptIsActive)

                {

                    // 1. 達到 5 秒，隱藏第一個提示 (例如：請將手保持在物品上五秒。)

                    hintSequencer.HideHint();

                    promptIsActive = false;

                    hintSequencer.ShowCongrats("非常好", 3.0f); // 顯示 "你好棒" 3 秒

                }



                // 停止計時，等待下次接觸1

                handIsTouching = false;

                touchTimer = 0f;



                Debug.Log("成功持續接觸 " + requiredTouchTime + " 秒，提示已消除，並顯示祝賀！");

            }

            else

            {
                Debug.Log("持續接觸時間: " + touchTimer.ToString("F2") + " 秒");
            }

        }

    }

    private void OnTriggerEnter(Collider other)

    {

        if (other.CompareTag(handTag))

        {

            if (!promptIsActive)

            {

                // 首次接觸，顯示提示文字 (顯示 "請把手移動到物品上面。")

                // 注意：這裡我們假設提示文字的內容由 HintSequencer 決定或提供

                hintSequencer.ShowHint("請將手保持在物品上五秒。", 9999f); // 設置一個極長的 duration，讓它不要自己計時消失

                promptIsActive = true;

            }



            // 開始接觸計時

            handIsTouching = true;

            touchTimer = 0f; // 每次進入都重新開始計時

        }

    }

    private void OnTriggerExit(Collider other)

    {

        if (other.CompareTag(handTag)== false)

        {

            // 中途離開，重置狀態和計時器1

            handIsTouching = false;

            touchTimer = 0f;

        }

    }

}