using UnityEngine;
using UnityEngine.UI;

// 修正：讓這個類別繼承 MonoBehaviour，並使用已宣告的 'slider' 欄位
public class SliderController : MonoBehaviour
{
    // 指向 UI Slider，請在 Inspector 指派，否則會嘗試自動取得同一個 GameObject 的 Slider 元件
    public Slider slider;
    public float speed = 1.0f;   // 滑桿移動速度
    private bool goingUp = true;
    public bool isRunning = false;  // 是否啟動滑桿

    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
    }

    void Update()
    {
        if (!isRunning || slider == null) return;

        if (goingUp)
        {
            slider.value += speed * Time.deltaTime;
            if (slider.value >= slider.maxValue) goingUp = false;
        }
        else
        {
            slider.value -= speed * Time.deltaTime;
            if (slider.value <= slider.minValue) goingUp = true;
        }
    }

    public void StopSlider()
    {
        isRunning = false;
    }
}
