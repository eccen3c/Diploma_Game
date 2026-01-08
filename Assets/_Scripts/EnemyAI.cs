using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Spawner mySpawner;
    public float spawnInterval = 3f; // Раз в 3 секунды враг думает
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRandomUnit();
            timer = 0;
        }
    }

    void SpawnRandomUnit()
    {
        // Бросаем монетку (0 или 1)
        int choice = Random.Range(0, 2);

        if (choice == 0)
        {
            // 50% шанс - Мечник
            mySpawner.BuySwordsman();
        }
        else
        {
            // 50% шанс - Лучник
            mySpawner.BuyArcher();
        }
    }
}