using UnityEngine;
using UnityEngine.Events;

public class HealthRestoreManager : MonoBehaviour
{
    [System.Serializable]
    public class RestoreEvent
    {
        public GameObject targetObject;
        public UnityEvent restoreMethod;
        public bool destroyOnContact = true; // Переменная для управления уничтожением объекта
        public AudioClip contactSound; // Звук при соприкосновении
    }

    public RestoreEvent[] restoreEvents;
    public AudioSource audioSource; // Аудио источник для воспроизведения звуков

    // Обработка контакта игрока с целью
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
                    Destroy(target); // Удаляем объект после контакта, если это разрешено
                }
                break; // Выход из цикла после восстановления правильной цели
            }
        }
    }
}
