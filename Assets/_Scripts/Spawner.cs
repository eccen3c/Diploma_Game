using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Настройки")]
    public Transform spawnPoint;
    public bool isEnemy = false;

    [Header("Юниты (Префабы)")]
    public GameObject swordPrefab;  // Сюда перетащишь Unit_Ally
    public GameObject archerPrefab; // Сюда перетащишь Unit_Archer_Ally

    [Header("Цены")]
    public int swordPrice = 100;
    public int archerPrice = 150;

    // --- ФУНКЦИИ ДЛЯ КНОПОК ИГРОКА ---

    public void BuySwordsman()
    {
        TrySpawn(swordPrefab, swordPrice);
    }

    public void BuyArcher()
    {
        TrySpawn(archerPrefab, archerPrice);
    }

    // --- ФУНКЦИЯ ДЛЯ ВРАГА (чтобы EnemyAI не ломался) ---
    public void SpawnSoldier()
    {
        // Враг пока будет спавнить только мечников
        TrySpawn(swordPrefab, 0);
    }

    // --- ВНУТРЕННЯЯ ЛОГИКА ---

    private void TrySpawn(GameObject unitToSpawn, int price)
    {
        // Если это ВРАГ, он спавнит бесплатно
        if (isEnemy)
        {
            CreateUnit(unitToSpawn);
            return;
        }

        // Если это ИГРОК, проверяем деньги
        if (GameManager.instance.gold >= price)
        {
            GameManager.instance.SpendGold(price);
            CreateUnit(unitToSpawn);
        }
        else
        {
            Debug.Log("Не хватает золота!");
        }
    }

    void CreateUnit(GameObject prefab)
    {
        // Случайная высота (чтобы не шли одной линией)
        float randomY = Random.Range(-2f, 2f);
        Vector3 pos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + randomY, 0);

        Instantiate(prefab, pos, Quaternion.identity);
    }
}