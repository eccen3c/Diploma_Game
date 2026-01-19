using UnityEngine;
using TMPro; // Для текста
using System.Collections; // Для корутин (таймеров)

public class RoundManager : MonoBehaviour
{
    [Header("Настройки Времени")]
    public float firstRoundTime = 10f;
    public float normalRoundTime = 30f;
    public float spawnInterval = 0.2f; // Как быстро вылетают юниты (пулеметная очередь)

    [Header("Экономика P1 (Слева)")]
    public int p1_Gold = 100;
    public int p1_Income = 50;
    public int p1_Supply = 60; // Лимит
    public TextMeshProUGUI p1_GoldText; // Ссылка на текст золота

    [Header("Экономика P2 (Справа)")]
    public int p2_Gold = 100;
    public int p2_Income = 50;
    public int p2_Supply = 60;
    public TextMeshProUGUI p2_GoldText;

    [Header("Ссылки на Систему")]
    public TextMeshProUGUI timerText;    // Текст таймера по центру
    public Transform spawnPointP1;       // Точка спавна левого
    public Transform spawnPointP2;       // Точка спавна правого

    // Нам нужны ссылки на Input, чтобы знать, КОГО выбрал игрок прямо сейчас
    public ShopInput inputP1;
    public ShopInput inputP2;
    public ShopManager shopManager;      // Чтобы брать данные юнитов

    private float currentTime;
    private bool isSpawning = false;     // Чтобы таймер не шел во время спавна (если нужно)

    void Start()
    {
        // Запускаем первый короткий раунд
        StartCoroutine(GameLoop());
    }

    // Главный цикл игры: Таймер -> Спавн -> Таймер -> Спавн...
    IEnumerator GameLoop()
    {
        // 1. Первый разгон (10 сек)
        currentTime = firstRoundTime;
        yield return StartCoroutine(TimerRoutine());

        // 2. Вечный цикл (30 сек)
        while (true)
        {
            // --- ФАЗА 00:00: СПАВН И ДОХОД ---
            yield return StartCoroutine(SpawnPhase());

            // --- ФАЗА ТАЙМЕРА ---
            currentTime = normalRoundTime;
            yield return StartCoroutine(TimerRoutine());
        }
    }

    IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateUI();
            yield return null; // Ждем следующий кадр
        }
        currentTime = 0;
        UpdateUI();
    }

    IEnumerator SpawnPhase()
    {
        // 1. Начисляем доход и сбрасываем саплай
        p1_Gold += p1_Income;
        p2_Gold += p2_Income;

        p1_Supply = 60; // Сброс лимита
        p2_Supply = 60;

        // 2. Потоковый спавн (тратим деньги, пока они есть)
        // Будем пытаться спавнить, пока ХОТЯ БЫ У ОДНОГО есть деньги и саплай
        bool p1CanBuy = true;
        bool p2CanBuy = true;

        while (p1CanBuy || p2CanBuy)
        {
            // --- ЛОГИКА ИГРОКА 1 ---
            // Берем юнита, который выбран ПРЯМО СЕЙЧАС (Dynamic Choice)
            UnitData unitP1 = GetCurrentUnit(inputP1);

            // Проверяем: хватает ли денег, места и есть ли вообще юнит
            if (unitP1 != null && p1_Gold >= unitP1.cost && p1_Supply >= unitP1.supplyCost)
            {
                SpawnUnit(unitP1, spawnPointP1, 1); // 1 = команда игрока
                p1_Gold -= unitP1.cost;             // Забираем деньги
                p1_Income += unitP1.incomeBonus;    // Увеличиваем доход!
                p1_Supply -= unitP1.supplyCost;     // Тратим саплай
            }
            else
            {
                p1CanBuy = false; // Деньги или саплай кончились
            }

            // --- ЛОГИКА ИГРОКА 2 ---
            UnitData unitP2 = GetCurrentUnit(inputP2);

            if (unitP2 != null && p2_Gold >= unitP2.cost && p2_Supply >= unitP2.supplyCost)
            {
                SpawnUnit(unitP2, spawnPointP2, 2);
                p2_Gold -= unitP2.cost;
                p2_Income += unitP2.incomeBonus;
                p2_Supply -= unitP2.supplyCost;
            }
            else
            {
                p2CanBuy = false;
            }

            UpdateUI(); // Обновляем цифры денег на экране

            // Ждем перед следующим спавном (эффект ручейка)
            yield return new WaitForSeconds(spawnInterval);

            // Если деньги кончились, цикл прервется сам
        }
    }

    // Вспомогательная функция: узнать, кто сейчас выбран в рамке
    UnitData GetCurrentUnit(ShopInput input)
    {
        int index = input.selectedIndex;
        // Проверяем, чтобы не выйти за пределы списка
        if (index < shopManager.allUnits.Count)
        {
            return shopManager.allUnits[index];
        }
        return null;
    }

    void SpawnUnit(UnitData data, Transform spawnPoint, int team)
    {
        if (data.unitPrefab == null) return;

        // 1. Создаем клона
        GameObject newUnit = Instantiate(data.unitPrefab, spawnPoint.position, Quaternion.identity);

        // 2. Настраиваем Команду (Team)
        if (team == 1) // Игрок 1 (Слева, Синий)
        {
            newUnit.tag = "Player"; // Твои юниты
            newUnit.layer = LayerMask.NameToLayer("Player"); // Если есть слой

            // Если нужно повернуть лицом вправо (стандарт)
            // newUnit.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (team == 2) // Игрок 2 (Справа, Враг)
        {
            newUnit.tag = "Enemy"; // Теперь это ВРАГ для первого игрока
            newUnit.layer = LayerMask.NameToLayer("Enemy"); // Слой врага

            // Красим спрайт в красный цвет, чтобы визуально отличать
            SpriteRenderer sr = newUnit.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.red;

            // Разворачиваем спрайт, чтобы смотрел влево (на игрока)
            // Если у тебя спрайты смотрят вправо по умолчанию
            Vector3 scale = newUnit.transform.localScale;
            scale.x = -Mathf.Abs(scale.x); // Делаем икс отрицательным
            newUnit.transform.localScale = scale;
        }

        // --- ВАЖНО ПРО АТАКУ ---
        // Тут нужно сказать скрипту AI, кого бить.
        // Если у тебя есть скрипт типа "UnitAI" или "AttackScript", раскомментируй и поправь:

        /*
        var ai = newUnit.GetComponent<UnitAI>(); // Вставь имя ТВОЕГО скрипта движения
        if (ai != null)
        {
            if (team == 1) ai.enemyTag = "Enemy"; // Игрок бьет Врагов
            else           ai.enemyTag = "Player"; // Враг бьет Игроков
        }
        */
    }

    void UpdateUI()
    {
        // Таймер (округляем до целых)
        if (timerText != null) timerText.text = Mathf.Ceil(currentTime).ToString();

        // Деньги и Инкам
        if (p1_GoldText != null) p1_GoldText.text = $"Gold: {p1_Gold}\nInc: +{p1_Income}";
        if (p2_GoldText != null) p2_GoldText.text = $"Gold: {p2_Gold}\nInc: +{p2_Income}";
    }
}