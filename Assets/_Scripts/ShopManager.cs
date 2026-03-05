using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Нужно для сортировки (по желанию)

public class ShopManager : MonoBehaviour
{
    [Header("Containers")]
    public Transform shopContainerP1; // Ссылка на Shop_P1_Left
    public Transform shopContainerP2; // Ссылка на Shop_P2_Right

    [Header("Data")]
    public List<UnitData> allUnits;   // Список всех загруженных юнитов

    private void Start()
    {
        LoadUnits();
        InitializeShop(shopContainerP1);
        InitializeShop(shopContainerP2);
    }

    void LoadUnits()
    {
        // 1. ПРОВЕРКА: Если ты уже добавил юнитов руками в Инспекторе (список не пуст)
        if (allUnits != null && allUnits.Count > 0)
        {
            Debug.Log("Используем ручной список из Инспектора (сортировка сохранена).");
            return; // ВЫХОДИМ ИЗ МЕТОДА. Дальше код не пойдет.
        }

        // 2. РЕЗЕРВ: Если список пуст, грузим автоматом (как было раньше)
        Debug.Log("Список пуст. Загружаем всё из папки Resources автоматически.");

        UnitData[] loaded = Resources.LoadAll<UnitData>("Units");

        // Если хочешь авто-сортировку по цене — раскомментируй строку ниже.
        // allUnits = loaded.OrderBy(u => u.cost).ToList(); 

        // А пока просто грузим как попало (или по алфавиту имен файлов)
        allUnits = loaded.ToList();

        Debug.Log($"Авто-загружено юнитов: {allUnits.Count}");
    }

    void InitializeShop(Transform container)
    {
        // Пробегаем по всем кнопкам (UnitSlot) внутри контейнера
        for (int i = 0; i < container.childCount; i++)
        {
            // Берем слот под номером i
            UnitSlotUI slot = container.GetChild(i).GetComponent<UnitSlotUI>();

            // Если у нас есть юнит для этого слота
            if (i < allUnits.Count)
            {
                slot.SetupSlot(allUnits[i]);
            }
            else
            {
                // Если юнитов меньше, чем слотов (например, всего 4 юнита, а слотов 16)
                // Можно выключить лишние слоты или оставить пустыми
                slot.gameObject.SetActive(false); // Раскомментируй, если хочешь скрыть пустые
            }
        }
    }
}