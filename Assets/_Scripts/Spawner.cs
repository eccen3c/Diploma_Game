using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject unitPrefab;
    public Transform spawnPoint;
    public int price = 30;

    [Header("Чей это спавнер?")]
    public bool isEnemy = false; // <-- Новая галочка

    public void SpawnSoldier()
    {
        if (isEnemy)
        {
            CreateUnit();
            return;
        }

        if (GameManager.instance.gold >= price)
        {
            GameManager.instance.SpendGold(price);
            CreateUnit();
        }
        else
        {
            Debug.Log("Не хватает золота!");
        }
    }

    void CreateUnit()
    {
        // Вычисляем случайную высоту (от -2 до +2 метров по Y)
        float randomY = Random.Range(-2f, 2f);

        // Создаем новую позицию спавна
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, spawnPoint.position.y + randomY, 0);

        Instantiate(unitPrefab, spawnPos, Quaternion.identity);
    }
}