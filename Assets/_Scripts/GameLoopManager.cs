using UnityEngine;
using TMPro;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager instance;

    [Header("Тайминги")]
    public float startDelay = 10f;
    public float roundDuration = 30f;
    public float spawnWindowDuration = 8f;

    private float currentTimer;
    private bool isWarmup = true;

    [Header("Поток Спавна")]
    public float spawnInterval = 0.2f;
    private float nextSpawnTime = 0;
    private bool isSpawnOpen = false;

    [Header("Игрок 1 (Слева)")]
    public int p1_Gold = 100;
    public int p1_Income = 50;
    public int p1_Supply = 60;
    public Transform p1_SpawnPoint;
    public TextMeshProUGUI p1_UI_Stats;

    [Header("Игрок 2 (Справа)")]
    public int p2_Gold = 100;
    public int p2_Income = 50;
    public int p2_Supply = 60;
    public Transform p2_SpawnPoint;
    public TextMeshProUGUI p2_UI_Stats;

    [Header("Ссылки")]
    public TextMeshProUGUI timerText;
    public ShopInput inputP1;
    public ShopInput inputP2;
    public ShopManager shopManager;

    void Awake() { instance = this; }

    void Start()
    {
        currentTimer = startDelay;
        isWarmup = true;
        UpdateUI();
    }

    void Update()
    {
        // 1. ТАЙМЕР
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
        }
        else
        {
            if (isWarmup)
            {
                isWarmup = false;
                StartNewRound();
            }
            else
            {
                StartNewRound();
            }
        }

        // 2. ОКНО СПАВНА
        if (!isWarmup && currentTimer > (roundDuration - spawnWindowDuration))
        {
            isSpawnOpen = true;
        }
        else
        {
            isSpawnOpen = false;
        }

        // 3. СПАВН
        if (isSpawnOpen && Time.time >= nextSpawnTime)
        {
            TrySpawn(1);
            TrySpawn(2);
            nextSpawnTime = Time.time + spawnInterval;
            UpdateUI();
        }

        if (timerText)
        {
            timerText.text = Mathf.Ceil(currentTimer).ToString();
            timerText.color = isSpawnOpen ? Color.green : Color.white;
        }
    }

    void StartNewRound()
    {
        currentTimer = roundDuration;
        p1_Gold += p1_Income;
        p2_Gold += p2_Income;
        p1_Supply = 60;
        p2_Supply = 60;
        UpdateUI();
    }

    void TrySpawn(int playerNum)
    {
        ShopInput input = (playerNum == 1) ? inputP1 : inputP2;
        Transform spawnPoint = (playerNum == 1) ? p1_SpawnPoint : p2_SpawnPoint;
        string tag = (playerNum == 1) ? "Player" : "Enemy";

        ref int gold = ref (playerNum == 1 ? ref p1_Gold : ref p2_Gold);
        ref int income = ref (playerNum == 1 ? ref p1_Income : ref p2_Income);
        ref int supply = ref (playerNum == 1 ? ref p1_Supply : ref p2_Supply);

        int index = input.selectedIndex;
        if (index < 0 || index >= shopManager.allUnits.Count) return;

        UnitData unit = shopManager.allUnits[index];

        if (gold >= unit.cost && supply >= unit.supplyCost)
        {
            gold -= unit.cost;
            supply -= unit.supplyCost;
            income += unit.incomeBonus;

            // Мы передаем номер игрока, чтобы выбрать правильный префаб
            CreateUnit(unit, spawnPoint, tag, playerNum);
        }
    }

    void CreateUnit(UnitData data, Transform point, string tag, int playerNum)
    {
        float randomY = Random.Range(-1.5f, 1.5f);
        Vector3 pos = new Vector3(point.position.x, point.position.y + randomY, 0);

        // --- ИСПРАВЛЕНИЕ ОШИБКИ ТУТ ---
        // Мы больше не ищем data.unitPrefab, мы выбираем конкретный:
        GameObject prefabToSpawn;

        if (playerNum == 1)
            prefabToSpawn = data.prefab_Player;
        else
            prefabToSpawn = data.prefab_Enemy;

        // Если забыл назначить префаб в UnitData — выходим
        if (prefabToSpawn == null) return;

        GameObject obj = Instantiate(prefabToSpawn, pos, Quaternion.identity);
        obj.tag = tag;
        obj.layer = LayerMask.NameToLayer(tag);

        UnitController ctrl = obj.GetComponent<UnitController>();
        if (ctrl != null) ctrl.SetupUnit(data);

        // Разворачиваем врага (если нужно)
        if (tag == "Enemy")
        {
            Vector3 scale = obj.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            obj.transform.localScale = scale;
        }
    }

    void UpdateUI()
    {
        if (p1_UI_Stats) p1_UI_Stats.text = $"{p1_Gold}\n+{p1_Income}\n{p1_Supply}/60";
        if (p2_UI_Stats) p2_UI_Stats.text = $"{p2_Gold}\n+{p2_Income}\n{p2_Supply}/60";
    }
}