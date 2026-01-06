using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Spawner enemySpawner; // Ссылка на спавнер врага
    public float spawnInterval = 3.0f; // Как часто спавнить (раз в 3 сек)

    private float timer;

    void Update()
    {
        // Тикаем таймером
        timer += Time.deltaTime;

        // Если прошло 3 секунды
        if (timer >= spawnInterval)
        {
            AttemptSpawn();
            timer = 0; // Сбрасываем таймер
        }
    }

    void AttemptSpawn()
    {
        // Тут можно добавить логику: "Если у игрока много войск — спавни больше"
        // Но пока просто спавним всегда
        enemySpawner.SpawnSoldier();
    }
}