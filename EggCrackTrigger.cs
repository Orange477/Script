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
        if (!hand.isTracked) return;

        if (handTransform.childCount > 0)
        {
            Transform grabbed = handTransform.GetChild(0);
            if (grabbed != null && grabbed.name == "egg")
            {
                Debug.Log("YEAHHHHHHH");
            }
            else
            {
                return;
            }
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
