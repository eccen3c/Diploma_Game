using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Экономика")]
    public int gold = 100;
    public int incomePerSecond = 10;
    public TextMeshProUGUI goldText;

    private float incomeTimer = 0f;

    [Header("Таймер Раунда")]
    public float roundDuration = 15f;
    public float currentTimer;
    public TextMeshProUGUI timerText;

    [Header("Ссылки")]
    public Spawner playerSpawner;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentTimer = roundDuration;
        UpdateUI();
    }

    void Update()
    {
        // 1. Золото (капает всегда)
        incomeTimer += Time.deltaTime;
        if (incomeTimer >= 1f)
        {
            AddGold(incomePerSecond);
            incomeTimer = 0f;
        }

        // 2. Таймер раунда
        currentTimer -= Time.deltaTime;

        if (currentTimer <= 0)
        {
            // ВРЕМЯ ВЫШЛО!
            StartNewWave();
        }

        // Обновляем текст
        if (timerText != null)
            timerText.text = Mathf.Ceil(Mathf.Max(0, currentTimer)).ToString();
    }

    void StartNewWave()
    {
        // Сбрасываем таймер обратно на 15
        currentTimer = roundDuration;

        // Даем команду спавнеру: "Трать всё, что есть!"
        if (playerSpawner != null)
        {
            playerSpawner.StartWave();
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold: " + gold;
    }
}