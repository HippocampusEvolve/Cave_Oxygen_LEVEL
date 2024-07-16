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
    [Tooltip("��� �������, � ������� ����� ����������������� �����")]
    public string tag;

    [Tooltip("��� �������������� � ����������")]
    public OxygenInteractionType interactionType;

    [Tooltip("�������� �������������� (��������, ���������� ��������� ��� ����������/����������)")]
    public float interactionValue;

    [Tooltip("������ ����������� ������ ��� ��������� �������")]
    public float detectionRadius;

    [Tooltip("����, ��������������� ��� ��������������")]
    public AudioClip interactionSound;

    [Tooltip("���������, ������������ ��� ��������� �������")]
    public string statusMessage;

    [Tooltip("������������ ������� ��� ��������� ����������� (� ��������)")]
    public float duration;

    [Tooltip("���������� ������ ����� ��������������")]
    public bool destroyAfterInteraction;

    [Tooltip("GameObject � UI ���������� ��� ����������� �������")]
    public GameObject uiElement;
}

public enum OxygenInteractionType
{
    [Tooltip("��������� �������� ��������� ���������")]
    ReduceDepletion,
    [Tooltip("��������� ����� ���������")]
    Replenish,
    [Tooltip("�������� ��������� ���������")]
    Deplete,
    [Tooltip("�������� ��������� ������������� ������������� ���������")]
    TemporaryBoost,
    [Tooltip("�������� ����������� ������ ���������")]
    TemporaryDrain,
    [Tooltip("��������� ��������� ������ �� ���������")]
    RandomEffect
}



public class OxygenManager : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [Tooltip("������������ ���������� ���������")]
    public float maxOxygen = 100f;
    [Tooltip("������� ���������� ���������")]
    public float currentOxygen;
    [Tooltip("������� �������� ��������� ���������")]
    public float baseOxygenDepletionRate = 1f;

    [Header("UI Elements")]
    [Tooltip("������� ��� ����������� ������ ���������")]
    public Slider oxygenSlider;
    [Tooltip("����� ��� ����������� �������� ���������")]
    public TextMeshProUGUI oxygenText;
    [Tooltip("����� ��� ����������� �������")]
    public TextMeshProUGUI statusText;

    [Header("Player Reference")]
    [Tooltip("������ �� ��������� ������")]
    public Transform player;

    [Header("Oxygen Elements")]
    [Tooltip("������ ��������� ���������")]
    public List<OxygenElement> oxygenElements = new List<OxygenElement>();

    [Header("Message Display Settings")]
    [Tooltip("����� ����������� ������� ��������� (� ��������)")]
    public float messageDisplayTime = 2f;
    [Tooltip("�������� ����� ����������� (� ��������)")]
    public float messageDelayTime = 0.5f;

    [Tooltip("������� �������� ��������� ���������")]
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

        // ���������, �� ����� �� ������ ReduceDepletion
        if (isReduceDepletionActive && Time.time > reduceDepletionEndTime)
        {
            isReduceDepletionActive = false;
            currentOxygenDepletionRate = baseOxygenDepletionRate;
        }

        // ������� �������� ��� UI ��������
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

                // ���������� UI �������, ��� ��� ����� ��������� � ���� ��������
                ShowUIElement(element);

                // ������������� ���� ������ ��� ����� ��������������
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

        // ��������� �������� ��������������
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

        // ���������� ������� ����� ���� ��������������
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
        // ������ ��� �������� � HandleOxygenInteractions
        // ����� ����� �������� �������������� ������, ���� ����������
    }
    private void ApplyElementEffect(OxygenElement element)
    {
        switch (element.interactionType)
        {
            case OxygenInteractionType.ReduceDepletion:
                // ��������� ������ ReduceDepletion, ���� ���� �� ��� �������
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

        // ��������� ������ � ������� �������� ��������
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
        Debug.Log("�������� ����������! ���� ��������.");
        // �������������� ������ ���������� ����
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

        UpdateStatusText(""); // ������� ������ ����� ����������� ���� ���������
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

