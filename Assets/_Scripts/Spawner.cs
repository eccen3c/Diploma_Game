using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject unitPrefab; // Ссылка на префаб (Кого создаем)
    public Transform spawnPoint;  // Ссылка на точку (Где создаем)

    // Эту функцию мы привяжем к кнопке
    public void SpawnSoldier()
    {
        // Instantiate - это команда "Создать копию"
        // Quaternion.identity означает "без вращения"
        Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
    }
}