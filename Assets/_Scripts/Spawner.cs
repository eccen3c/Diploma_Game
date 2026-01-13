using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Настройки")]
    public Transform spawnPoint;
    public GameObject unitPrefab; // Ссылка на Скелета (потом заменим на массив префабов)

    [Header("Баланс")]
    public float spawnInterval = 0.15f; // Задержка между выходом юнитов

    // --- НОВЫЕ НАСТРОЙКИ РАЗБРОСА ---
    [Header("Разброс Спавна")]
    public float spreadX = 1.5f; // Разброс влево-вправо (чтобы не шли по струнке)
    public float spreadY = 2.5f; // Разброс вверх-вниз (ширина дороги)

    [Header("UI Кнопки")]
    public GameObject[] slots;

    private int currentSelectionIndex = 0; // Где сейчас курсор

    void Start()
    {
        SetSelection(0);
    }

    // --- ГЛАВНЫЙ МЕТОД (Вызывает GameManager раз в 15 сек) ---
    public void StartWave()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        // Бесконечный цикл внутри одной волны? Нет, мы спавним пока есть деньги.
        // Как только деньги кончились - выходим из корутины и ждем следующего вызова через 15 сек.

        while (true)
        {
            // 1. Смотрим, какой юнит сейчас выбран (пока везде скелет unitPrefab)
            // В будущем тут будет: GameObject prefabToSpawn = unitPrefabs[currentSelectionIndex];
            GameObject prefabToSpawn = unitPrefab;

            if (prefabToSpawn == null) break;

            UnitStats stats = prefabToSpawn.GetComponent<UnitStats>();

            // 2. Пытаемся купить
            if (GameManager.instance.SpendGold(stats.cost))
            {
                // 3. Деньги были -> Спавним с РАЗБРОСОМ
                SpawnUnit(prefabToSpawn);

                // 4. Ждем перед следующим
                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                // 5. Денег НЕТ -> Волна закончена. Ждем следующего таймера.
                Debug.Log("Золото кончилось, спавн остановлен.");
                break; // Выход из цикла
            }
        }
    }

    void SpawnUnit(GameObject prefab)
    {
        // Используем наши переменные для рандома
        float offsetX = Random.Range(-spreadX, spreadX);
        float offsetY = Random.Range(-spreadY, spreadY);

        Vector3 randomPos = new Vector3(
            spawnPoint.position.x + offsetX,
            spawnPoint.position.y + offsetY,
            0
        );

        Instantiate(prefab, randomPos, Quaternion.identity);
    }

// --- УПРАВЛЕНИЕ КУРСОРОМ (WASD)
public void SetSelection(int index)
    {
        currentSelectionIndex = index;
        for (int i = 0; i < slots.Length; i++)
        {
            Transform frame = slots[i].transform.Find("SelectionFrame");
            if (frame != null) frame.gameObject.SetActive(i == index);
        }
    }
}