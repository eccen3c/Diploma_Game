using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public ShopInput shopInputP2;

    private GameLoopManager loop;
    private bool spawnWindowWasOpen = false;

    void Start()
    {
        if (GameSession.mode != GameMode.SoloVsBot)
        {
            enabled = false;
            return;
        }

        loop = GameLoopManager.instance;
    }

    void Update()
    {
        if (loop == null) return;

        bool windowOpen = IsSpawnWindowOpen();

        if (windowOpen && !spawnWindowWasOpen)
            OnSpawnWindowOpened();

        spawnWindowWasOpen = windowOpen;
    }

    bool IsSpawnWindowOpen()
    {
        // GameLoopManager не даёт прямой доступ к isSpawnOpen — определяем по таймеру
        // roundDuration - spawnWindowDuration = порог открытия окна
        float threshold = loop.roundDuration - loop.spawnWindowDuration;
        return !IsWarmup() && loop.GetCurrentTimer() > threshold;
    }

    bool IsWarmup()
    {
        return loop.GetIsWarmup();
    }

    void OnSpawnWindowOpened()
    {
        float delay = GetReactionDelay();
        StartCoroutine(DecideAfterDelay(delay));
    }

    IEnumerator DecideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsSpawnWindowOpen())
            ApplyDecision(MakeDecision());
    }

    int MakeDecision()
    {
        List<UnitData> units = loop.shopManager.allUnits;
        if (units == null || units.Count == 0) return 0;

        return GameSession.difficulty switch
        {
            BotDifficulty.Easy   => PickRandom(units),
            BotDifficulty.Medium => PickByValue(units),
            BotDifficulty.Hard   => PickCounter(units),
            _                    => 0
        };
    }

    void ApplyDecision(int index)
    {
        if (shopInputP2 != null)
            shopInputP2.selectedIndex = index;
    }

    float GetReactionDelay()
    {
        return GameSession.difficulty switch
        {
            BotDifficulty.Easy   => Random.Range(4f, 5f),
            BotDifficulty.Medium => Random.Range(1.5f, 2.5f),
            BotDifficulty.Hard   => Random.Range(0f, 0.5f),
            _                    => 1f
        };
    }

    // Easy: случайный юнит, который можно позволить
    int PickRandom(List<UnitData> units)
    {
        List<int> affordable = units
            .Select((u, i) => (u, i))
            .Where(x => loop.p2_Gold >= x.u.cost)
            .Select(x => x.i)
            .ToList();

        if (affordable.Count == 0) return 0;
        return affordable[Random.Range(0, affordable.Count)];
    }

    // Medium: случайный из топ-50% по score (hp * damage / cost)
    int PickByValue(List<UnitData> units)
    {
        var scored = units
            .Select((u, i) => (u, i, score: u.cost > 0 ? (u.hp * u.damage) / u.cost : 0f))
            .Where(x => loop.p2_Gold >= x.u.cost)
            .OrderByDescending(x => x.score)
            .ToList();

        if (scored.Count == 0) return 0;

        int topCount = Mathf.Max(1, scored.Count / 2);
        int pick = Random.Range(0, topCount);
        return scored[pick].i;
    }

    // Hard: контрпик под армию игрока
    int PickCounter(List<UnitData> units)
    {
        GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("Player");

        if (playerUnits.Length == 0)
            return PickByValue(units);

        float avgPlayerDamage = 0f;
        float avgPlayerHp = 0f;
        int count = 0;

        foreach (var obj in playerUnits)
        {
            UnitData data = GetUnitData(obj);
            if (data == null) continue;
            avgPlayerDamage += data.damage;
            avgPlayerHp += data.hp;
            count++;
        }

        if (count == 0) return PickByValue(units);

        avgPlayerDamage /= count;
        avgPlayerHp /= count;

        var affordable = units
            .Select((u, i) => (u, i))
            .Where(x => loop.p2_Gold >= x.u.cost)
            .ToList();

        if (affordable.Count == 0) return 0;

        // Если у игрока высокий урон относительно hp → они агрессивные → берём самого живучего
        // Если у игрока низкий урон (танки) → берём самого дамажного
        float offenseRatio = avgPlayerDamage / Mathf.Max(avgPlayerHp, 1f);
        if (offenseRatio > 0.1f)
            return affordable.OrderByDescending(x => x.u.hp).First().i;
        else
            return affordable.OrderByDescending(x => x.u.damage).First().i;
    }

    UnitData GetUnitData(GameObject obj)
    {
        UnitController ctrl = obj.GetComponent<UnitController>();
        if (ctrl != null) return ctrl.unitData;

        TestUnitAI ai = obj.GetComponent<TestUnitAI>();
        if (ai != null) return ai.unitData;

        TestArcherAI archer = obj.GetComponent<TestArcherAI>();
        if (archer != null) return archer.unitData;

        return null;
    }
}
