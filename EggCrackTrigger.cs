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
    private int a = 0;
    void Update()
    { 
        if (isCracked || hand == null || handTransform == null) return;
        if(!hand.isTracked) return;
        // ✅ 只有當蛋是被抓取的物件時，才開始偵測揮動
        if (GrabTrigger.heldObject == this.gameObject)
        {
            if (a < 100)
            {
                Debug.Log(GrabTrigger.heldObject);
                a++;
            }
            else
            {
                Debug.Log(" 蛋被抓取中 ，你太強了");
            }
        }
        else
        {
            Debug.Log(GrabTrigger.heldObject);
            Debug.Log(this.gameObject);
            Debug.Log("一定是黃騰毅的錯");
            return;
        }


        if (DetectDownwardSwing())
        {
            CrackEgg();
        }
    }

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

        return velocityY < -0.5f;
    }

    void CrackEgg()
    {
        isCracked = true;
        gameObject.SetActive(false);

        if (uncookedEggPrefab != null && panTransform != null)
        {
            Instantiate(uncookedEggPrefab, panTransform.position, Quaternion.identity);
        }

        Debug.Log("蛋已打入鍋中！");
    }
}
