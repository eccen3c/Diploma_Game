using UnityEngine;
using TMPro;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameLoopManager : MonoBehaviourPunCallbacks
{
    public static GameLoopManager instance;

    [Header("Налаштування часу")]
    public float startDelay = 10f;
    public float roundDuration = 30f;
    public float spawnWindowDuration = 8f;

    private float currentTimer;
    private bool isWarmup = true;

    [Header("Таймінг спавну")]
    public float spawnInterval = 0.2f;
    private float nextSpawnTime = 0;
    private bool isSpawnOpen = false;

    [Header("Гравець 1 (Лівий / Сині)")]
    public int p1_Gold = 1000;
    public int p1_Income = 300;
    public int p1_Supply = 60;
    public Transform p1_SpawnPoint;
    public TextMeshProUGUI p1_UI_Stats;

    [Header("Гравець 2 (Правий / Червоні)")]
    public int p2_Gold = 1000;
    public int p2_Income = 300;
    public int p2_Supply = 60;
    public Transform p2_SpawnPoint;
    public TextMeshProUGUI p2_UI_Stats;

    [Header("Інтерфейс та Менеджери")]
    public TextMeshProUGUI timerText;
    public ShopInput inputP1;
    public ShopInput inputP2;
    public ShopManager shopManager;

    [Header("Синхронізація Прискорення (2x / 4x)")]
    private int p1_RequestedSpeed = 1;
    private int p2_RequestedSpeed = 1;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentTimer = startDelay;
        isWarmup = true;
        UpdateUI();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (currentTimer > 0)
                {
                    currentTimer -= Time.deltaTime;
                    if (Time.frameCount % 5 == 0)
                    {
                        Hashtable roomTime = new Hashtable {
                            { "RoomTimer", currentTimer },
                            { "IsWarmup", isWarmup },
                            { "CurrentTimeScale", Time.timeScale } // Передаємо швидкість гри клієнту
                        };
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomTime);
                    }
                }
                else
                {
                    photonView.RPC("RPC_StartNewRound", RpcTarget.All);
                }
            }
        }
        else
        {
            if (currentTimer > 0) currentTimer -= Time.deltaTime;
            else StartNewRound();
        }

        // Твоя логіка перевірки вікна спавну
        isSpawnOpen = (!isWarmup && currentTimer > (roundDuration - spawnWindowDuration));

        if (isSpawnOpen && Time.time >= nextSpawnTime)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    TrySpawnUnitByIndex(1);
                    TrySpawnUnitByIndex(2);
                }
            }
            else
            {
                TrySpawnUnitByIndex(1);
                TrySpawnUnitByIndex(2);
            }

            nextSpawnTime = Time.time + spawnInterval;
            UpdateUI();
        }

        if (timerText)
        {
            timerText.text = Mathf.Ceil(currentTimer).ToString();
            timerText.color = isSpawnOpen ? Color.green : Color.white;
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (propertiesThatChanged.ContainsKey("RoomTimer")) currentTimer = (float)propertiesThatChanged["RoomTimer"];
            if (propertiesThatChanged.ContainsKey("IsWarmup")) isWarmup = (bool)propertiesThatChanged["IsWarmup"];
            if (propertiesThatChanged.ContainsKey("CurrentTimeScale"))
            {
                Time.timeScale = (float)propertiesThatChanged["CurrentTimeScale"];
            }
        }
    }

    [PunRPC]
    void RPC_StartNewRound()
    {
        StartNewRound();
    }

    void StartNewRound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayScream();
        isWarmup = false;
        currentTimer = roundDuration;
        p1_Gold += p1_Income;
        p2_Gold += p2_Income;
        p1_Supply = 60;
        p2_Supply = 60;
        UpdateUI();
    }

    #region МЕРЕЖЕВЕ ПРИСКОРЕННЯ (2x / 4x)

    // Метод для кнопок UI швидкості
    public void RequestGameSpeed(int targetSpeed)
    {
        int myPlayerNum = PhotonNetwork.IsMasterClient ? 1 : 2;
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            photonView.RPC("RPC_RegisterSpeedVote", RpcTarget.All, myPlayerNum, targetSpeed);
        }
        else
        {
            Time.timeScale = targetSpeed;
        }
    }

    [PunRPC]
    void RPC_RegisterSpeedVote(int playerNum, int votedSpeed)
    {
        if (playerNum == 1) p1_RequestedSpeed = votedSpeed;
        if (playerNum == 2) p2_RequestedSpeed = votedSpeed;

        if (PhotonNetwork.IsMasterClient)
        {
            // Гра прискориться тільки якщо обидва обрали однакову швидкість
            if (p1_RequestedSpeed == p2_RequestedSpeed)
            {
                Time.timeScale = p1_RequestedSpeed;
            }
            else
            {
                Time.timeScale = 1f; // Якщо думки розійшлися — повертаємо дефолтну 1х
            }
        }
    }

    #endregion

    [PunRPC]
    public void RPC_SelectUnit(int playerNum, int slotIndex)
    {
        LocalSelect(playerNum, slotIndex);
    }

    public void LocalSelect(int playerNum, int slotIndex)
    {
        ShopInput input = (playerNum == 1) ? inputP1 : inputP2;
        Transform container = (playerNum == 1) ? shopManager.shopContainerP1 : shopManager.shopContainerP2;

        if (input != null && container != null && slotIndex >= 0 && slotIndex < container.childCount)
        {
            input.selectedIndex = slotIndex;

            // Визначаємо, хто я в мережі: Хост (1) чи Гість (2)
            int myLocalPlayerNum = 1;
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                myLocalPlayerNum = 2;
            }

            for (int i = 0; i < container.childCount; i++)
            {
                UnitSlotUI slot = container.GetChild(i).GetComponent<UnitSlotUI>();
                if (slot != null)
                {
                    if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                    {
                        // Якщо ми в мережі, рамка увімкнеться ТІЛЬКИ якщо цей вибір 
                        // належить поточному локальному гравцю
                        if (playerNum == myLocalPlayerNum)
                        {
                            slot.SetSelected(i == slotIndex);
                        }
                        else
                        {
                            // Для чужого вибору рамку примусово вимикаємо/не показуємо
                            slot.SetSelected(false);
                        }
                    }
                    else
                    {
                        // Якщо граємо в локальному режимі (офлайн) — показуємо рамки як зазвичай
                        slot.SetSelected(i == slotIndex);
                    }
                }
            }
        }
    }

    void TrySpawnUnitByIndex(int playerNum)
    {
        Transform container = (playerNum == 1) ? shopManager.shopContainerP1 : shopManager.shopContainerP2;
        ShopInput input = (playerNum == 1) ? inputP1 : inputP2;

        if (container == null || input == null) return;

        int index = input.selectedIndex;
        if (index < 0 || index >= container.childCount) return;

        UnitSlotUI slot = container.GetChild(index).GetComponent<UnitSlotUI>();
        if (slot == null || slot.unitData == null) return;

        UnitData unit = slot.unitData;

        int gold = (playerNum == 1) ? p1_Gold : p2_Gold;
        int supply = (playerNum == 1) ? p1_Supply : p2_Supply;

        if (gold >= unit.cost && supply >= unit.supplyCost)
        {
            if (playerNum == 1)
            {
                p1_Gold -= unit.cost;
                p1_Supply -= unit.supplyCost;
                p1_Income += unit.incomeBonus;
            }
            else
            {
                p2_Gold -= unit.cost;
                p2_Supply -= unit.supplyCost;
                p2_Income += unit.incomeBonus;
            }

            ExecuteSpawn(unit, playerNum);
        }
    }

    void ExecuteSpawn(UnitData data, int playerNum)
    {
        Transform spawnPoint = (playerNum == 1) ? p1_SpawnPoint : p2_SpawnPoint;
        if (spawnPoint == null) return;

        GameObject targetPrefab = (playerNum == 1) ? data.prefab_Player : data.prefab_Enemy;
        if (targetPrefab == null) return;

        float randomY = Random.Range(-1.5f, 1.5f);
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + randomY, 0);

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            string prefabName = targetPrefab.name.Replace("(Clone)", "").Trim();

            // Твоя авто-корекція назв (щоб не було помилок DefaultPool)
            prefabName = prefabName.Replace("_", "-");
            if (prefabName.EndsWith("-Blue")) prefabName = prefabName.Substring(0, prefabName.Length - 5) + "-BLUE";
            if (prefabName.EndsWith("-Red")) prefabName = prefabName.Substring(0, prefabName.Length - 4) + "-RED";
            if (prefabName.EndsWith("-blue")) prefabName = prefabName.Substring(0, prefabName.Length - 5) + "-BLUE";
            if (prefabName.EndsWith("-red")) prefabName = prefabName.Substring(0, prefabName.Length - 4) + "-RED";

            string resourcePath = prefabName;

            try
            {
                // Спавнимо з дефолтним поворотом префабу!
                GameObject networkUnit = PhotonNetwork.Instantiate(resourcePath, spawnPos, Quaternion.identity);

                // ПЕРЕДАЄМО ПАРАМЕТР playerNum (1 або 2), щоб юніт сам розібрався, куди дивитися
                networkUnit.GetComponent<UnitController>()?.SetupUnit(data, playerNum);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Photon Spawn Error] Не знайдено префаб: Resources/{resourcePath}");
            }
        }
        else
        {
            // Локальний тест
            GameObject localUnit = Instantiate(targetPrefab, spawnPos, Quaternion.identity);
            localUnit.GetComponent<UnitController>()?.SetupUnit(data, playerNum);
        }
    }

    void UpdateUI()
    {
        // Твій оригінальний дизайн відображення статів з пробілами
        if (p1_UI_Stats != null)
            p1_UI_Stats.text = $"{p1_Gold}                +{p1_Income}                {p1_Supply}/60";

        if (p2_UI_Stats != null)
            p2_UI_Stats.text = $"{p2_Gold}                +{p2_Income}                {p2_Supply}/60";
    }
}