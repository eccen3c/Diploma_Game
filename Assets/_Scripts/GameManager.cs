using UnityEngine;
using TMPro; // Подключаем библиотеку для текста

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Ссылка на самого себя

    [Header("Экономика")]
    public int gold = 100;        // Начальное золото
    public int passiveIncome = 5; // Сколько дают денег
    public float incomeRate = 1f; // Как часто (раз в секунду)

    [Header("UI")]
    public TextMeshProUGUI goldText; // Ссылка на текст на экране

    void Awake()
    {
        // Магия Синглтона: говорим, что "Главный тут я"
        instance = this;
    }

    void Start()
    {
        UpdateUI();
        // Запускаем бесконечный таймер начисления денег
        InvokeRepeating("AddPassiveGold", 1f, incomeRate);
    }

    void AddPassiveGold()
    {
        gold += passiveIncome;
        UpdateUI();
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        // Обновляем текст на экране
        if (goldText != null)
            goldText.text = "Gold: " + gold.ToString();
    }
}