using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;
    public float speed = 1.0f;   // 滑桿移動速度
    private bool goingUp = true;
    public bool isRunning = false;  // 是否啟動滑桿

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
