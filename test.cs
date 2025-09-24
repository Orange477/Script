using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // 引用 UI

public enum GestureType
{
    None,
    Grab,       // 抓取食材或工具
    Stir,       // 翻炒動作
    Pour,       // 傾倒調味料
    Release     // 釋放物件
}

// 定義烹飪步驟
public enum CookingStep
{
    PrepareIngredients,
    AddToPan,
    StirFry,
    Season,
    Complete
}

// 定義食物熟度狀態
public enum FoodState
{
    Raw,
    Cooking,
    Cooked,
    Overcooked
}

public class CookingGameManager : MonoBehaviour
{
    // 虛擬雙手的物件
    public Transform leftHand;
    public Transform rightHand;

    // 食材的 3D 模型
    public GameObject rawFoodModel;
    public GameObject cookingFoodModel;
    public GameObject cookedFoodModel;
    public GameObject overcookedFoodModel;
    private GameObject currentFoodModel;

    // 食譜步驟
    private CookingStep currentStep = CookingStep.PrepareIngredients;
    private Dictionary<CookingStep, string> stepInstructions = new Dictionary<CookingStep, string>
    {
        { CookingStep.PrepareIngredients, "請抓取食材並放入鍋中" },
        { CookingStep.AddToPan, "將食材放入鍋中" },
        { CookingStep.StirFry, "開始翻炒食材" },
        { CookingStep.Season, "添加調味料" },
        { CookingStep.Complete, "烹飪完成！" }
    };

    // 食物熟度相關參數
    private float cookingTime = 0f;
    private float requiredCookingTime = 10f;
    private float overcookThreshold = 15f;
    private FoodState foodState = FoodState.Raw;

    // 手勢數據（暫時用鍵盤模擬）
    private Vector3 leftHandPosition;
    private Vector3 rightHandPosition;
    private GestureType currentGesture = GestureType.None;

    // 翻炒動作計數
    private int stirCount = 0;
    private int requiredStirCount = 5;
    private float lastStirTime = 0f;
    private float stirCooldown = 0.5f;

    // 新增：調味 Panel + Slider
    public GameObject seasonPanel;
    public Slider seasonSlider;
    private SliderController sliderController;

    void Start()
    {
        Debug.Log(stepInstructions[currentStep]);
        SetFoodModel(foodState);

        if (seasonSlider != null)
            sliderController = seasonSlider.GetComponent<SliderController>();

        if (seasonPanel != null)
            seasonPanel.SetActive(false); // 一開始隱藏
    }

    void Update()
    {
        UpdateHandPositions();
        DetectGesture();

        switch (currentStep)
        {
            case CookingStep.PrepareIngredients:
                if (currentGesture == GestureType.Grab)
                {
                    Debug.Log("食材已抓取，進入下一步");
                    currentStep = CookingStep.AddToPan;
                    Debug.Log(stepInstructions[currentStep]);
                }
                break;

            case CookingStep.AddToPan:
                if (currentGesture == GestureType.Release && IsHandNearPan())
                {
                    Debug.Log("食材已放入鍋中，開始翻炒");
                    currentStep = CookingStep.StirFry;
                    Debug.Log(stepInstructions[currentStep]);
                }
                break;

            case CookingStep.StirFry:
                cookingTime += Time.deltaTime;
                UpdateFoodState();

                if (currentGesture == GestureType.Stir && Time.time - lastStirTime > stirCooldown)
                {
                    stirCount++;
                    lastStirTime = Time.time;
                    Debug.Log($"翻炒次數: {stirCount}/{requiredStirCount}");

                    if (stirCount >= requiredStirCount)
                    {
                        Debug.Log("翻炒完成，進入調味步驟");
                        currentStep = CookingStep.Season;
                        Debug.Log(stepInstructions[currentStep]);

                        // 顯示調味 Panel & 啟動滑桿
                        if (seasonPanel != null)
                        {
                            seasonPanel.SetActive(true);
                            if (sliderController != null) sliderController.isRunning = true;
                        }
                    }
                }
                break;

            case CookingStep.Season:
                if (currentGesture == GestureType.Pour)
                {
                    if (sliderController != null)
                        sliderController.StopSlider();

                    float flavorValue = seasonSlider.value;
                    Debug.Log($"調味完成！最終數值: {flavorValue}");

                    // 👉 例子：判斷調味是否完美
                    if (flavorValue >= 0.4f && flavorValue <= 0.6f)
                        Debug.Log("完美調味！");
                    else
                        Debug.Log("調味不理想...");

                    if (seasonPanel != null)
                        seasonPanel.SetActive(false);

                    currentStep = CookingStep.Complete;
                    Debug.Log(stepInstructions[currentStep]);
                }
                break;

            case CookingStep.Complete:
                Debug.Log("遊戲結束，食物狀態: " + foodState);
                break;
        }

        SetFoodModel(foodState);
    }

    // ====== 輔助方法 ======
    void UpdateHandPositions()
    {
        leftHandPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x - 50, Input.mousePosition.y, 10));
        rightHandPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + 50, Input.mousePosition.y, 10));

        leftHand.position = leftHandPosition;
        rightHand.position = rightHandPosition;
    }

    void DetectGesture()
    {
        if (Input.GetKey(KeyCode.G))
            currentGesture = GestureType.Grab;
        else if (Input.GetKey(KeyCode.S))
            currentGesture = GestureType.Stir;
        else if (Input.GetKey(KeyCode.P))
            currentGesture = GestureType.Pour;
        else if (Input.GetKeyUp(KeyCode.G))
            currentGesture = GestureType.Release;
        else
            currentGesture = GestureType.None;
    }

    bool IsHandNearPan()
    {
        Vector3 panPosition = Vector3.zero;
        float distanceThreshold = 2f;
        return Vector3.Distance(rightHandPosition, panPosition) < distanceThreshold ||
               Vector3.Distance(leftHandPosition, panPosition) < distanceThreshold;
    }

    void UpdateFoodState()
    {
        if (cookingTime < requiredCookingTime)
            foodState = FoodState.Cooking;
        else if (cookingTime < overcookThreshold)
            foodState = FoodState.Cooked;
        else
            foodState = FoodState.Overcooked;
    }

    void SetFoodModel(FoodState state)
    {
        if (currentFoodModel != null)
            currentFoodModel.SetActive(false);

        switch (state)
        {
            case FoodState.Raw: currentFoodModel = rawFoodModel; break;
            case FoodState.Cooking: currentFoodModel = cookingFoodModel; break;
            case FoodState.Cooked: currentFoodModel = cookedFoodModel; break;
            case FoodState.Overcooked: currentFoodModel = overcookedFoodModel; break;
        }

        if (currentFoodModel != null)
            currentFoodModel.SetActive(true);
    }
}
