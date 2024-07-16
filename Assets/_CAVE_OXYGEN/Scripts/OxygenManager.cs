using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[Serializable]
public class OxygenElement
{
    [Tooltip("Тег объекта, с которым будет взаимодействовать игрок")]
    public string tag;

    [Tooltip("Тип взаимодействия с кислородом")]
    public OxygenInteractionType interactionType;

    [Tooltip("Значение взаимодействия (например, количество кислорода для пополнения/уменьшения)")]
    public float interactionValue;

    [Tooltip("Радиус обнаружения игрока для активации эффекта")]
    public float detectionRadius;

    [Tooltip("Звук, воспроизводимый при взаимодействии")]
    public AudioClip interactionSound;

    [Tooltip("Сообщение, отображаемое при активации эффекта")]
    public string statusMessage;

    [Tooltip("Длительность эффекта для временных воздействий (в секундах)")]
    public float duration;

    [Tooltip("Уничтожать объект после взаимодействия")]
    public bool destroyAfterInteraction;

    [Tooltip("GameObject с UI элементами для отображения эффекта")]
    public GameObject uiElement;
}

public enum OxygenInteractionType
{
    [Tooltip("Уменьшает скорость истощения кислорода")]
    ReduceDepletion,
    [Tooltip("Пополняет запас кислорода")]
    Replenish,
    [Tooltip("Ускоряет истощение кислорода")]
    Deplete,
    [Tooltip("Временно усиливает эффективность использования кислорода")]
    TemporaryBoost,
    [Tooltip("Временно увеличивает расход кислорода")]
    TemporaryDrain,
    [Tooltip("Применяет случайный эффект из доступных")]
    RandomEffect
}



public class OxygenManager : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [Tooltip("Максимальное количество кислорода")]
    public float maxOxygen = 100f;
    [Tooltip("Текущее количество кислорода")]
    public float currentOxygen;
    [Tooltip("Базовая скорость истощения кислорода")]
    public float baseOxygenDepletionRate = 1f;

    [Header("UI Elements")]
    [Tooltip("Слайдер для отображения уровня кислорода")]
    public Slider oxygenSlider;
    [Tooltip("Текст для отображения процента кислорода")]
    public TextMeshProUGUI oxygenText;
    [Tooltip("Текст для отображения статуса")]
    public TextMeshProUGUI statusText;

    [Header("Player Reference")]
    [Tooltip("Ссылка на трансформ игрока")]
    public Transform player;

    [Header("Oxygen Elements")]
    [Tooltip("Список элементов кислорода")]
    public List<OxygenElement> oxygenElements = new List<OxygenElement>();

    [Header("Message Display Settings")]
    [Tooltip("Время отображения каждого сообщения (в секундах)")]
    public float messageDisplayTime = 2f;
    [Tooltip("Задержка между сообщениями (в секундах)")]
    public float messageDelayTime = 0.5f;

    [Tooltip("Текущая скорость истощения кислорода")]
    private float currentOxygenDepletionRate;

    private AudioSource audioSource;
    private Dictionary<OxygenInteractionType, float> temporaryEffects = new Dictionary<OxygenInteractionType, float>();
    private Queue<string> messageQueue = new Queue<string>();
    private bool isDisplayingMessages = false;
    private Dictionary<GameObject, OxygenElement> activeInteractions = new Dictionary<GameObject, OxygenElement>();
    private Dictionary<OxygenElement, float> activeEffects = new Dictionary<OxygenElement, float>();

    private bool isReduceDepletionActive = false;
    private float reduceDepletionEndTime = 0f;

    void Start()
    {
        InitializeOxygen();
        UpdateUI();
        audioSource = gameObject.AddComponent<AudioSource>();
        currentOxygenDepletionRate = baseOxygenDepletionRate;
    }

    void Update()
    {
        HandleOxygenInteractions();
        UpdateTemporaryEffects();
        UpdateUI();
    }

    private void InitializeOxygen()
    {
        currentOxygen = maxOxygen;
    }

    private void HandleOxygenInteractions()
    {
        HashSet<string> uniqueMessages = new HashSet<string>();
        Dictionary<GameObject, OxygenElement> newInteractions = new Dictionary<GameObject, OxygenElement>();
        List<GameObject> objectsToDestroy = new List<GameObject>();

        // Проверяем, не истек ли эффект ReduceDepletion
        if (isReduceDepletionActive && Time.time > reduceDepletionEndTime)
        {
            isReduceDepletionActive = false;
            currentOxygenDepletionRate = baseOxygenDepletionRate;
        }

        // Сначала скрываем все UI элементы
        foreach (var interaction in activeInteractions)
        {
            HideUIElement(interaction.Value);
        }

        foreach (var element in oxygenElements)
        {
            GameObject interactedObject = IsPlayerNearElement(element);
            if (interactedObject != null)
            {
                newInteractions[interactedObject] = element;
                ApplyElementEffect(element);
                uniqueMessages.Add(element.statusMessage);

                // Показываем UI элемент, так как игрок находится в зоне действия
                ShowUIElement(element);

                // Воспроизводим звук только при новом взаимодействии
                if (!activeInteractions.ContainsKey(interactedObject))
                {
                    PlaySound(element.interactionSound);
                }

                if (element.destroyAfterInteraction)
                {
                    objectsToDestroy.Add(interactedObject);
                }
            }
        }

        // Обновляем активные взаимодействия
        activeInteractions = newInteractions;

        foreach (var message in uniqueMessages)
        {
            EnqueueMessage(message);
        }

        ApplyTemporaryEffects(ref currentOxygenDepletionRate);
        DepleteOxygen(Time.deltaTime * currentOxygenDepletionRate);

        if (!isDisplayingMessages)
        {
            StartCoroutine(DisplayMessagesCoroutine());
        }

        if (IsOxygenDepleted())
        {
            OnOxygenDepleted();
        }

        // Уничтожаем объекты после всех взаимодействий
        foreach (var obj in objectsToDestroy)
        {
            Destroy(obj);
            activeInteractions.Remove(obj);
        }
    }

    private GameObject IsPlayerNearElement(OxygenElement element)
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(player.position, element.detectionRadius);
        foreach (Collider obj in nearbyObjects)
        {
            if (obj.CompareTag(element.tag))
            {
                return obj.gameObject;
            }
        }
        return null;
    }
    private void ApplyReduceDepletionEffect(OxygenElement element)
    {
        // Эффект уже применен в HandleOxygenInteractions
        // Здесь можно добавить дополнительную логику, если необходимо
    }
    private void ApplyElementEffect(OxygenElement element)
    {
        switch (element.interactionType)
        {
            case OxygenInteractionType.ReduceDepletion:
                // Применяем эффект ReduceDepletion, даже если он уже активен
                float reductionFactor = 1f - (element.interactionValue / 100f);
                currentOxygenDepletionRate = baseOxygenDepletionRate * reductionFactor;
                isReduceDepletionActive = true;
                reduceDepletionEndTime = Time.time + element.duration;
                break;
            case OxygenInteractionType.Replenish:
                AddOxygen(element.interactionValue);
                break;
            case OxygenInteractionType.Deplete:
                DepleteOxygen(element.interactionValue);
                break;
            case OxygenInteractionType.TemporaryBoost:
            case OxygenInteractionType.TemporaryDrain:
                ApplyTemporaryEffect(element);
                break;
            case OxygenInteractionType.RandomEffect:
                ApplyRandomEffect(element);
                break;
        }

        // Добавляем эффект в словарь активных эффектов
        activeEffects[element] = Time.time + element.duration;
    }
    private IEnumerator ResetDepletionRateAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        currentOxygenDepletionRate = baseOxygenDepletionRate;
        isReduceDepletionActive = false;
    }
    private void ApplyTemporaryEffect(OxygenElement element)
    {
        temporaryEffects[element.interactionType] = Time.time + element.duration;
    }

    private void ApplyRandomEffect(OxygenElement element)
    {
        float randomValue = UnityEngine.Random.value;
        if (randomValue < 0.4f)
        {
            AddOxygen(element.interactionValue);
        }
        else if (randomValue < 0.8f)
        {
            DepleteOxygen(element.interactionValue);
        }
        else
        {
            ApplyTemporaryEffect(element);
        }
    }

    private void UpdateTemporaryEffects()
    {
        List<OxygenElement> expiredEffects = new List<OxygenElement>();
        foreach (var effect in activeEffects)
        {
            if (Time.time > effect.Value)
            {
                expiredEffects.Add(effect.Key);
                HideUIElement(effect.Key);
            }
        }
        foreach (var expiredEffect in expiredEffects)
        {
            activeEffects.Remove(expiredEffect);
        }
    }

    private void ShowUIElement(OxygenElement element)
    {
        if (element.uiElement != null)
        {
            element.uiElement.SetActive(true);
        }
    }

    private void HideUIElement(OxygenElement element)
    {
        if (element.uiElement != null)
        {
            element.uiElement.SetActive(false);
        }
    }

    private void ApplyTemporaryEffects(ref float currentDepletionRate)
    {
        if (temporaryEffects.ContainsKey(OxygenInteractionType.TemporaryBoost))
        {
            currentDepletionRate *= 0.5f;
        }
        if (temporaryEffects.ContainsKey(OxygenInteractionType.TemporaryDrain))
        {
            currentDepletionRate *= 1.5f;
        }
    }

    public void DepleteOxygen(float amount)
    {
        currentOxygen = Mathf.Clamp(currentOxygen - amount, 0, maxOxygen);
    }

    public void AddOxygen(float amount)
    {
        currentOxygen = Mathf.Clamp(currentOxygen + amount, 0, maxOxygen);
    }

    private bool IsOxygenDepleted()
    {
        return currentOxygen <= 0;
    }

    private void OnOxygenDepleted()
    {
        Debug.Log("Кислород закончился! Игра окончена.");
        // Дополнительная логика завершения игры
    }

    private void UpdateUI()
    {
        if (oxygenSlider != null)
        {
            oxygenSlider.value = currentOxygen / maxOxygen;
        }
        if (oxygenText != null)
        {
            oxygenText.text = $"{(currentOxygen / maxOxygen * 100):F1}%";
        }
    }

    private void EnqueueMessage(string message)
    {
        if (!messageQueue.Contains(message))
        {
            messageQueue.Enqueue(message);
        }
    }

    private IEnumerator DisplayMessagesCoroutine()
    {
        isDisplayingMessages = true;

        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            UpdateStatusText(message);
            yield return new WaitForSeconds(messageDisplayTime);

            if (messageQueue.Count > 0)
            {
                yield return new WaitForSeconds(messageDelayTime);
            }
        }

        UpdateStatusText(""); // Очищаем статус после отображения всех сообщений
        isDisplayingMessages = false;
    }

    private void UpdateStatusText(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

