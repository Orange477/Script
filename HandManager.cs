using UnityEngine;
using System.Collections.Concurrent;

public class HandManager : MonoBehaviour
{
    public static HandManager instance;

    [Header("���ⱱ��]�y��^")]
    public SimpleHandController leftHandController;

    [Header("�k�ⱱ��]�y��^")]
    public SimpleHandController rightHandController;

    private ConcurrentQueue<HandData[]> handDataQueue = new ConcurrentQueue<HandData[]>();

    // �s�W�G�s�ثe����B�k��y��
    private Landmark[] currentLeftLandmarks;
    private Landmark[] currentRightLandmarks;

    [System.Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class HandData
    {
        public string handedness;
        public Landmark[] landmarks;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void EnqueueHandData(HandData[] hands)
    {
        handDataQueue.Enqueue(hands);
    }

    void Update()
    {
        while (handDataQueue.TryDequeue(out HandData[] hands))
        {
            foreach (var hand in hands)
            {
                if (hand.handedness == "Left" && leftHandController != null)
                {
                    leftHandController.ApplyLandmarks(hand.landmarks);
                    currentLeftLandmarks = hand.landmarks; // �O����y��
                }
                else if (hand.handedness == "Right" && rightHandController != null)
                {
                    rightHandController.ApplyLandmarks(hand.landmarks);
                    currentRightLandmarks = hand.landmarks; // �O��k��y��
                }
            }
        }
    }

    // �s�W�G���o�Y���⪺ landmark
    public Landmark[] GetLandmarks(bool isLeftHand = true)
    {
        return isLeftHand ? currentLeftLandmarks : currentRightLandmarks;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            try
            {
                string newJson = "{\"array\":" + json + "}";
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
                return wrapper.array;
            }
            catch (System.Exception e)
            {
                Debug.LogError("JSON Parsing Error: " + e.Message);
                return null;
            }
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
