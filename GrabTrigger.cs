using UnityEngine;
using System.Collections.Generic;

public class GrabTrigger : MonoBehaviour
{
    public float grabRange = 10f; // ����Z��
    public Transform handTransform; // �⪺ Transform
    public HandManager handManager; // �ⳡ�޲z���A���Ѥ���I��T

    private GameObject heldObject = null;
    private static GameObject globallyHeldObject = null; // �����ߤ@�Q�쪫��
    private Dictionary<GameObject, (Vector3 pos, Quaternion rot)> originalTransforms = new Dictionary<GameObject, (Vector3, Quaternion)>();

    private void Start()
    {
        // �ƥ����Ҧ��� Ladle ���Ҫ����~�A�O���L�̪���l��m�M����
        var ladles = GameObject.FindGameObjectsWithTag("Ladle");
        foreach (var ladle in ladles)
        {
            originalTransforms[ladle] = (ladle.transform.position, ladle.transform.rotation);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (heldObject != null) return; // �w������~�ɤ��~���

        Transform current = other.transform;
        GameObject target = null;

        // ��즳 Ladle ���Ҫ�������
        while (current != null)
        {
            if (current.CompareTag("Ladle"))
            {
                target = current.gameObject;
                break;
            }
            current = current.parent;
        }

        if (target == null) return;

        Vector3 handPos = handTransform.position;
        Vector3 targetPos = target.transform.position;

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null) return;

        Vector3 closestPointOnTarget = targetCollider.ClosestPoint(handPos);
        float distance = Vector3.Distance(handPos, closestPointOnTarget);

        if (distance > grabRange) return; // �W�L����d�򤣧�

        if (IsPinching())
        {
            GrabObject(target);
        }
    }

    private void Update()
    {
        // ��}���~����G�w��B���A���X
        if (heldObject != null && !IsPinching())
        {
            ReleaseObject();
        }
    }

    private void GrabObject(GameObject target)
    {
        if (globallyHeldObject != null) return; // ��L��w�쪫��ɸ��L

        heldObject = target;
        globallyHeldObject = target;

        heldObject.transform.SetParent(handTransform);

        // ���~��m�k�s�]�۹�⪺��m�^
        heldObject.transform.localPosition = Vector3.zero;

        // ���~����]�w�����P�B�]�i�L�ա^
        heldObject.transform.rotation = handTransform.rotation;
        // �Y���~��V���A�i�H�ոեH�U�L�ը���
        heldObject.transform.rotation = handTransform.rotation * Quaternion.Euler(-90, 0, 0);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("����G" + heldObject.name);
    }

    private void ReleaseObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("��}�G" + heldObject.name);

        heldObject = null;
        globallyHeldObject = null;
    }

    private bool IsPinching()
    {
        if (handManager == null) return false;

        var landmarks = handManager.GetLandmarks();
        if (landmarks == null || landmarks.Length < 9) return false;

        Vector3 thumb = new Vector3(landmarks[4].x, landmarks[4].y, landmarks[4].z);
        Vector3 index = new Vector3(landmarks[8].x, landmarks[8].y, landmarks[8].z);

        float pinchDist = Vector3.Distance(thumb, index);

        Debug.Log($"����P�����Z��: {pinchDist}, �O�_���: {pinchDist < 0.3f}");

        return pinchDist < 0.3f;
    }
}
