using UnityEngine;

public class EggCrackTrigger : MonoBehaviour
{
    [Header("手部偵測")]
    public HandManager handManager;
    public Transform handTransform;

    [Header("蛋模型")]
    public GameObject uncookedEggPrefab; // 指定 uncooked.fbx 預製物
    public Transform panTransform;       // 指定平底鍋的位置

    private bool isCracked = false;

    void Update()
    {
        if (isCracked) return;

        // 只偵測上下揮動
        if (DetectDownwardSwing())
        {
            CrackEgg();
        }
    }

    // 偵測食指和中指向上，其他向下
    bool DetectCrackGesture()
    {
        var landmarks = handManager.GetLandmarks(handTransform.name.Contains("Left"));
        if (landmarks == null || landmarks.Length < 21) return false;

        // 食指(8)和中指(12) tip 高於掌心(0)，其他(16, 20)低於掌心
        float palmY = landmarks[0].y;
        bool indexUp = landmarks[8].y > palmY;
        bool middleUp = landmarks[12].y > palmY;
        bool ringDown = landmarks[16].y < palmY;
        bool pinkyDown = landmarks[20].y < palmY;

        return indexUp && middleUp && ringDown && pinkyDown;
    }

    // 偵測手由上往下揮（y軸速度為負）
    Vector3 lastHandPos;
    float lastTime;

    bool DetectDownwardSwing()
    {
        Vector3 currentPos = handTransform.position;
        float currentTime = Time.time;

        float velocityY = 0f;
        if (lastTime > 0f)
        {
            velocityY = (currentPos.y - lastHandPos.y) / (currentTime - lastTime);
        }

        lastHandPos = currentPos;
        lastTime = currentTime;

        // y軸速度小於 -0.5 視為揮下
        return velocityY < -0.5f;
    }

    void CrackEgg()
    {
        isCracked = true;
        // 隱藏原蛋
        gameObject.SetActive(false);

        // 生成未熟蛋到平底鍋
        if (uncookedEggPrefab != null && panTransform != null)
        {
            Instantiate(uncookedEggPrefab, panTransform.position, Quaternion.identity);
        }

        Debug.Log("蛋已打入鍋中！");
    }
}