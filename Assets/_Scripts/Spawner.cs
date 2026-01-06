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
        // 1. Если это враг — спавним бесплатно (пока что)
        if (isEnemy)
        {
            Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
            return; // Выходим из функции, дальше код не читаем
        }

        // 2. Если это игрок — проверяем деньги
        if (GameManager.instance.gold >= price)
        {
            GameManager.instance.SpendGold(price);
            Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Не хватает золота!");
        }
    }
}