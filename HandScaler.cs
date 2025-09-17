using UnityEngine;
using Leap;
using Leap.Unity;

public class SafeHandScaler : MonoBehaviour
{
    public HandModelBase leftHandModel;
    public HandModelBase rightHandModel;

    public Transform leftUnityHand;
    public Transform rightUnityHand;

    public float positionScale = 2.0f;

    void Update()
    {
        // ¥ª¤â
        if (leftHandModel != null && leftUnityHand != null)
        {
            Hand leapHand = leftHandModel.GetLeapHand();
            if (leapHand != null)
            {
                Vector3 palmPos = new Vector3(
                    leapHand.PalmPosition.x,
                    leapHand.PalmPosition.y,
                    leapHand.PalmPosition.z
                ) * positionScale;

                if (IsValidVector(palmPos))
                    leftUnityHand.localPosition = palmPos;
            }
        }

        // ¥k¤â
        if (rightHandModel != null && rightUnityHand != null)
        {
            Hand leapHand = rightHandModel.GetLeapHand();
            if (leapHand != null)
            {
                Vector3 palmPos = new Vector3(
                    leapHand.PalmPosition.x,
                    leapHand.PalmPosition.y,
                    leapHand.PalmPosition.z
                ) * positionScale;

                if (IsValidVector(palmPos))
                    rightUnityHand.localPosition = palmPos;
            }
        }
    }

    bool IsValidVector(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}