using UnityEngine;
using UnityEngine.Events;

public class HealthRestoreManager : MonoBehaviour
{
    [System.Serializable]
    public class RestoreEvent
    {
        public GameObject targetObject;
        public UnityEvent restoreMethod;
        public bool destroyOnContact = true; // ���������� ��� ���������� ������������ �������
        public AudioClip contactSound; // ���� ��� ���������������
    }

    public RestoreEvent[] restoreEvents;
    public AudioSource audioSource; // ����� �������� ��� ��������������� ������

    // ��������� �������� ������ � �����
    public void OnPlayerContact(GameObject target)
    {
        foreach (var restoreEvent in restoreEvents)
        {
            if (restoreEvent.targetObject == target)
            {
                restoreEvent.restoreMethod.Invoke();

                if (restoreEvent.contactSound != null)
                {
                    audioSource.PlayOneShot(restoreEvent.contactSound);
                }

                if (restoreEvent.destroyOnContact)
                {
                    Destroy(target); // ������� ������ ����� ��������, ���� ��� ���������
                }
                break; // ����� �� ����� ����� �������������� ���������� ����
            }
        }
    }
}
