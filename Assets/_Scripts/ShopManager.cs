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
        // Автоматически загружаем все файлы UnitData из папки Resources/Units
        UnitData[] loaded = Resources.LoadAll<UnitData>("Units");

        // Превращаем массив в список и сортируем по цене (от дешевых к дорогим)
        allUnits = loaded.OrderBy(u => u.cost).ToList();

        Debug.Log($"Загружено юнитов: {allUnits.Count}");
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