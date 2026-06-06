using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameLoopManager : MonoBehaviourPunCallbacks
{
    public static GameLoopManager instance;

    [Header("��������")]
    public float startDelay = 10f;
    public float roundDuration = 30f;
    public float spawnWindowDuration = 8f;

    private float currentTimer;
    private bool isWarmup = true;
    private bool isFirstRound = true;

    [Header("����� ������")]
    public float spawnInterval = 0.2f;
    private float nextSpawnTime = 0;
    private bool isSpawnOpen = false;

    [Header("����� 1 (�����)")]
    public int p1_Gold = 100;
    public int p1_Income = 50;
    public int p1_Supply = 60;
    public Transform p1_SpawnPoint;
    public TextMeshProUGUI p1_UI_Stats;

    [Header("����� 2 (������)")]
    public int p2_Gold = 100;
    public int p2_Income = 50;
    public int p2_Supply = 60;
    public Transform p2_SpawnPoint;
    public TextMeshProUGUI p2_UI_Stats;

    [Header("������")]
    public TextMeshProUGUI timerText;
    public ShopInput inputP1;
    public ShopInput inputP2;
    public ShopManager shopManager;

    // --- Online sync ---
    private Dictionary<int, GameObject> netUnits = new Dictionary<int, GameObject>();
    private int p1NetIdCounter = 0;
    private int p2NetIdCounter = 0;
    private float netSyncTimer = 0f;
    private const float NET_SYNC_INTERVAL = 0.1f;

    void Awake() { instance = this; }

    public float GetCurrentTimer() => currentTimer;
    public bool GetIsWarmup() => isWarmup;

    void Start()
    {
        if (PhotonNetwork.InRoom)
            GameSession.mode = GameMode.OnlineMulti;

        currentTimer = startDelay;
        isWarmup = true;
        UpdateUI();
    }

    void Update()
    {
        if (GameSession.mode == GameMode.OnlineMulti)
            UpdateOnline();
        else
            UpdateLocal();
    }

    void UpdateLocal()
    {
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayScream();

            isWarmup = false;
            StartNewRound();
        }

        UpdateSpawnWindow();

        if (isSpawnOpen && Time.time >= nextSpawnTime)
        {
            TrySpawn(1);
            TrySpawn(2);
            nextSpawnTime = Time.time + spawnInterval;
            UpdateUI();
        }

        UpdateTimerUI();
    }

    void UpdateOnline()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentTimer > 0)
            {
                currentTimer -= Time.deltaTime;
            }
            else
            {
                photonView.RPC("RPC_StartNewRound", RpcTarget.All);
                currentTimer = roundDuration; // запобігає повторному виклику
            }

            if (Time.frameCount % 5 == 0)
            {
                Hashtable props = new Hashtable { { "Timer", currentTimer }, { "IsWarmup", isWarmup } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }

            UpdateSpawnWindow();

            if (isSpawnOpen && Time.time >= nextSpawnTime)
            {
                TrySpawnOnline(1);
                nextSpawnTime = Time.time + spawnInterval;
            }

            netSyncTimer += Time.deltaTime;
            if (netSyncTimer >= NET_SYNC_INTERVAL)
            {
                netSyncTimer = 0f;
                SyncNetUnits();
            }
        }
        else
        {
            var props = PhotonNetwork.CurrentRoom?.CustomProperties;
            if (props != null)
            {
                if (props.ContainsKey("Timer")) currentTimer = (float)props["Timer"];
                if (props.ContainsKey("IsWarmup")) isWarmup = (bool)props["IsWarmup"];
            }

            UpdateSpawnWindow();

            if (isSpawnOpen && Time.time >= nextSpawnTime)
            {
                TrySpawnOnline(2);
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        UpdateUI();
        UpdateTimerUI();
    }

    void UpdateSpawnWindow()
    {
        isSpawnOpen = !isWarmup && currentTimer > (roundDuration - spawnWindowDuration);
    }

    void UpdateTimerUI()
    {
        if (timerText)
        {
            timerText.text = Mathf.Ceil(currentTimer).ToString();
            timerText.color = isSpawnOpen ? Color.green : Color.white;
        }
    }

    void TrySpawnOnline(int playerNum)
    {
        ShopInput input = (playerNum == 1) ? inputP1 : inputP2;
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
            float spawnY = Random.Range(-1.5f, 1.5f);
            photonView.RPC("RPC_SpawnUnit", RpcTarget.All, index, playerNum, spawnY);

            int g = playerNum == 1 ? p1_Gold : p2_Gold;
            int inc = playerNum == 1 ? p1_Income : p2_Income;
            int sup = playerNum == 1 ? p1_Supply : p2_Supply;
            photonView.RPC("RPC_SyncResources", RpcTarget.Others, playerNum, g, inc, sup);

            UpdateUI();
        }
    }

    [PunRPC]
    void RPC_SyncResources(int playerNum, int gold, int income, int supply)
    {
        if (playerNum == 1) { p1_Gold = gold; p1_Income = income; p1_Supply = supply; }
        else { p2_Gold = gold; p2_Income = income; p2_Supply = supply; }
        UpdateUI();
    }

    [PunRPC]
    void RPC_SpawnUnit(int unitIndex, int playerNum, float spawnY)
    {
        UnitData unit = shopManager.allUnits[unitIndex];
        Transform spawnPoint = (playerNum == 1) ? p1_SpawnPoint : p2_SpawnPoint;
        string tag = (playerNum == 1) ? "Player" : "Enemy";
        CreateUnit(unit, spawnPoint, tag, playerNum, spawnY);
    }

    [PunRPC]
    void RPC_StartNewRound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayScream();

        isWarmup = false;
        currentTimer = roundDuration;

        if (!isFirstRound)
        {
            p1_Gold += p1_Income;
            p2_Gold += p2_Income;
        }
        isFirstRound = false;
        p1_Supply = 60;
        p2_Supply = 60;
        UpdateUI();
    }

    void StartNewRound()
    {
        currentTimer = roundDuration;
        if (!isFirstRound)
        {
            p1_Gold += p1_Income;
            p2_Gold += p2_Income;
        }
        isFirstRound = false;
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

            // �� �������� ����� ������, ����� ������� ���������� ������
            CreateUnit(unit, spawnPoint, tag, playerNum, Random.Range(-1.5f, 1.5f));
        }
    }

    void CreateUnit(UnitData data, Transform point, string tag, int playerNum, float spawnY = 0f)
    {
        Vector3 pos = new Vector3(point.position.x, point.position.y + spawnY, 0);

        // --- ����������� ������ ��� ---
        // �� ������ �� ���� data.unitPrefab, �� �������� ����������:
        GameObject prefabToSpawn;

        if (playerNum == 1)
            prefabToSpawn = data.prefab_Player;
        else
            prefabToSpawn = data.prefab_Enemy;

        // ���� ����� ��������� ������ � UnitData � �������
        if (prefabToSpawn == null) return;

        GameObject obj = Instantiate(prefabToSpawn, pos, Quaternion.identity);
        obj.tag = tag;
        obj.layer = LayerMask.NameToLayer(tag);

        UnitController ctrl = obj.GetComponent<UnitController>();
        if (ctrl != null) { ctrl.SetupUnit(data); }
        else
        {
            TestUnitAI ai = obj.GetComponent<TestUnitAI>();
            if (ai != null) ai.SetupUnit(data);
            else
            {
                TestArcherAI archer = obj.GetComponent<TestArcherAI>();
                if (archer != null) archer.SetupUnit(data);
            }
        }

        // ������������� ����� (���� �����)
        if (tag == "Enemy")
        {
            Vector3 scale = obj.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            obj.transform.localScale = scale;
        }

        if (GameSession.mode == GameMode.OnlineMulti)
        {
            int netId = playerNum * 100000 + (playerNum == 1 ? p1NetIdCounter++ : p2NetIdCounter++);
            var nuid = obj.AddComponent<NetUnitId>();
            nuid.id = netId;
            netUnits[netId] = obj;

            if (!PhotonNetwork.IsMasterClient)
                obj.AddComponent<NetPositionInterpolator>();
        }
    }

    void UpdateUI()
    {
        if (p1_UI_Stats) p1_UI_Stats.text = $"{p1_Gold}\n+{p1_Income}\n{p1_Supply}/60";
        if (p2_UI_Stats) p2_UI_Stats.text = $"{p2_Gold}\n+{p2_Income}\n{p2_Supply}/60";
    }

    // --- Network unit sync ---

    void SyncNetUnits()
    {
        var ids = new List<int>();
        var xs = new List<float>();
        var ys = new List<float>();
        var toRemove = new List<int>();

        foreach (var kvp in netUnits)
        {
            if (kvp.Value == null) { toRemove.Add(kvp.Key); continue; }
            ids.Add(kvp.Key);
            xs.Add(kvp.Value.transform.position.x);
            ys.Add(kvp.Value.transform.position.y);
        }
        foreach (var id in toRemove) netUnits.Remove(id);

        if (ids.Count == 0) return;
        photonView.RPC("RPC_SyncUnits", RpcTarget.Others, ids.ToArray(), xs.ToArray(), ys.ToArray());
    }

    [PunRPC]
    void RPC_SyncUnits(int[] ids, float[] xs, float[] ys)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            if (!netUnits.TryGetValue(ids[i], out GameObject unit) || unit == null) continue;
            var interp = unit.GetComponent<NetPositionInterpolator>();
            if (interp != null)
                interp.SetTarget(new Vector3(xs[i], ys[i], 0f));
            else
                unit.transform.position = new Vector3(xs[i], ys[i], 0f);
        }
    }

    public void NotifyUnitDied(int netId)
    {
        if (GameSession.mode != GameMode.OnlineMulti || !PhotonNetwork.IsMasterClient) return;
        netUnits.Remove(netId);
        photonView.RPC("RPC_DestroyUnit", RpcTarget.Others, netId);
    }

    [PunRPC]
    void RPC_DestroyUnit(int netId)
    {
        netUnits.TryGetValue(netId, out GameObject unit);
        netUnits.Remove(netId);
        if (unit == null) return;

        var ai = unit.GetComponent<TestUnitAI>();
        if (ai != null) { ai.TriggerClientDeath(); return; }
        var archer = unit.GetComponent<TestArcherAI>();
        if (archer != null) { archer.TriggerClientDeath(); return; }
        var ctrl = unit.GetComponent<UnitController>();
        if (ctrl != null) { ctrl.TriggerClientDeath(); return; }
        Destroy(unit);
    }

    public void SyncUnitAttack(int netId)
    {
        if (GameSession.mode != GameMode.OnlineMulti || !PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_UnitAttack", RpcTarget.Others, netId);
    }

    [PunRPC]
    void RPC_UnitAttack(int netId)
    {
        if (!netUnits.TryGetValue(netId, out GameObject unit) || unit == null) return;
        var ai = unit.GetComponent<TestUnitAI>();
        if (ai != null) { ai.PlayClientAttack(); return; }
        var archer = unit.GetComponent<TestArcherAI>();
        if (archer != null) { archer.PlayClientAttack(); return; }
        var ctrl = unit.GetComponent<UnitController>();
        if (ctrl != null) ctrl.PlayClientAttack();
    }

    // --- Unit projectile sync ---

    public void SyncUnitProjectile(int shooterNetId, Vector2 spawnPos, Vector2 dir, float damage, string targetTag, Vector2 targetPos)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_UnitProjectile", RpcTarget.Others,
            shooterNetId,
            spawnPos.x, spawnPos.y,
            dir.x, dir.y,
            damage, targetTag,
            targetPos.x, targetPos.y);
    }

    [PunRPC]
    void RPC_UnitProjectile(int shooterNetId, float spawnX, float spawnY, float dirX, float dirY, float damage, string targetTag, float targetX, float targetY)
    {
        if (!netUnits.TryGetValue(shooterNetId, out GameObject shooter) || shooter == null) return;
        TestArcherAI archer = shooter.GetComponent<TestArcherAI>();
        if (archer == null || archer.arrowPrefab == null) return;

        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        Vector2 dir = new Vector2(dirX, dirY);
        Vector2 targetPos = new Vector2(targetX, targetY);

        GameObject arrowGO = Instantiate(archer.arrowPrefab, spawnPos, Quaternion.identity);
        Arrow arrow = arrowGO.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.isCosmetic = true;
            arrow.SetCosmeticDest(targetPos);
            arrow.Init(dir, damage, targetTag);
        }
    }

    // --- Base shot sync ---

    public void SyncBaseShot(bool isPlayerBase, Vector3 spawnPos, string ownerTag, int targetNetId, Vector3 fallbackPos)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_BaseShot", RpcTarget.Others,
            isPlayerBase,
            spawnPos.x, spawnPos.y,
            ownerTag, targetNetId,
            fallbackPos.x, fallbackPos.y);
    }

    [PunRPC]
    void RPC_BaseShot(bool isPlayerBase, float spawnX, float spawnY, string ownerTag, int targetNetId, float fallbackX, float fallbackY)
    {
        BaseController[] bases = FindObjectsOfType<BaseController>();
        BaseController shooter = null;
        foreach (var b in bases)
            if (b.isPlayerBase == isPlayerBase) { shooter = b; break; }
        if (shooter == null || shooter.projectilePrefab == null) return;

        AudioManager.Instance?.PlayFireball();
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);
        GameObject proj = Instantiate(shooter.projectilePrefab, spawnPos, Quaternion.identity);
        ProjectileController pc = proj.GetComponent<ProjectileController>();
        if (pc == null) return;

        pc.isCosmetic = true;

        if (targetNetId >= 0 && netUnits.TryGetValue(targetNetId, out GameObject unit) && unit != null)
            pc.SetTarget(unit.transform, ownerTag);
        else
            pc.SetFallbackTarget(new Vector3(fallbackX, fallbackY, 0f));
    }

    // --- Base HP sync ---

    public void SyncBaseHP(bool isPlayerBase, int hp)
    {
        if (GameSession.mode != GameMode.OnlineMulti) return;
        photonView.RPC("RPC_BaseHP", RpcTarget.Others, isPlayerBase, hp);
    }

    [PunRPC]
    void RPC_BaseHP(bool isPlayerBase, int hp)
    {
        BaseController[] bases = FindObjectsOfType<BaseController>();
        foreach (var b in bases)
            if (b.isPlayerBase == isPlayerBase)
                b.SetHP(hp);
    }

    // --- Game Over sync ---

    public void TriggerGameOver(string loserTag)
    {
        photonView.RPC("RPC_GameOver", RpcTarget.All, loserTag);
    }

    [PunRPC]
    void RPC_GameOver(string loserTag)
    {
        if (GameManager.instance) GameManager.instance.GameOver(loserTag);
    }
}