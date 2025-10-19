using UnityEngine;
using Leap.Unity.Interaction;

public class EggCrackTrigger : MonoBehaviour
{
    [Header("手部與手模型")]
    public InteractionHand hand;
    public Transform handTransform;

    [Header("蛋模型")]
    public GameObject uncookedEggPrefab;
    public Transform panTransform;
    private bool isCracked = false;
    private Vector3 lastHandPos;
    private float lastTime;
    private bool wasMovingUp = false; // 判斷是否先往上
    private int a = 0;
    private bool isEggInHand = false;

    [Header("抓取點")]
    public Transform grabPoint;

    void Update()
    {
        
        if (isCracked)
        {
            Debug.Log("打出來了");
            return;
        }

        // 每次都重設
        isEggInHand = false;
        Debug.Log("子物件數量" +grabPoint.childCount);
        // 判斷是否抓到蛋
        for (int i = 0; i < grabPoint.childCount; i++)
        {
            Transform child = grabPoint.GetChild(i);
            Debug.Log("forloop 執行中");
            if (child.CompareTag("egg"))
            {
                isEggInHand = true;
                Debug.Log("有瞜");
                break;
            }
        }

        if (!isEggInHand)
        {
            Debug.Log("手上沒有蛋，無法打蛋");
           return;
        } 

        if (DetectDownwardSwing())
        {
            CrackEgg();
        }
}

    bool DetectDownwardSwing()
    {
        Debug.Log("打蛋偵測執行中");
        Vector3 currentPos = handTransform.position;
        float currentTime = Time.time;

        float velocityY = 0f;
        if (lastTime > 0f)
        {
            velocityY = (currentPos.y - lastHandPos.y) / (currentTime - lastTime);
            Debug.Log("velocityY: " + velocityY + ", wasMovingUp: " + wasMovingUp);
            if (velocityY < -3.0f)
            { 
                return true;
            }
        }

        lastHandPos = currentPos;
        lastTime = currentTime;
        return false;
    }

    void CrackEgg()
    {
        Debug.Log("開打");
        if (isCracked) return;
        isCracked = true;
        gameObject.SetActive(false);

        if (uncookedEggPrefab != null && panTransform != null)
        {
            Instantiate(uncookedEggPrefab, panTransform.position, Quaternion.identity);
        }
        Debug.Log("蛋已打入鍋中！");
    }
}