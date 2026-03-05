using UnityEngine;

public class ShopInput : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }

    [Header("Settings")]
    public PlayerID playerID;       // Выбираем в инспекторе: P1 или P2
    public Transform gridContainer; // Ссылка на Shop_P1_Left или Shop_P2_Right

    [Header("State")]
    public int selectedIndex = 0;   // Какой слот выбран сейчас (0, 1, 2...)
    private int maxIndex;           // Сколько всего открыто юнитов

    // Ссылка на менеджер, чтобы знать, сколько юнитов доступно
    private ShopManager shopManager;

    void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();

        // Ждем немного, чтобы ShopManager успел загрузить данные
        // Но лучше просто взять количество сразу, если ShopManager работает в Awake
        // Для надежности обновим данные в первом кадре Update
    }

    void Update()
    {
        // 1. Узнаем, сколько у нас вообще юнитов (чтобы не уйти в пустоту)
        // Если ShopManager еще не прогрузился или список пуст - ничего не делаем
        if (shopManager == null || shopManager.allUnits == null) return;
        maxIndex = shopManager.allUnits.Count - 1;

        // 2. Читаем управление
        if (playerID == PlayerID.Player1)
        {
            if (Input.GetKeyDown(KeyCode.D)) MoveSelection(1);       // Вправо
            if (Input.GetKeyDown(KeyCode.A)) MoveSelection(-1);      // Влево
            if (Input.GetKeyDown(KeyCode.W)) MoveSelection(-8);      // Вверх (ряд -8)
            if (Input.GetKeyDown(KeyCode.S)) MoveSelection(8);       // Вниз (ряд +8)
        }
        else // Player 2
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSelection(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSelection(-1);
            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSelection(-8);
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSelection(8);
        }

        // 3. Обновляем визуал (включаем рамку)
        UpdateVisuals();
    }

    void MoveSelection(int change)
    {
        int newIndex = selectedIndex + change;

        // Проверки, чтобы не выйти за границы
        if (newIndex >= 0 && newIndex <= maxIndex)
        {
            selectedIndex = newIndex;
        }
    }

    void UpdateVisuals()
    {
        // Пробегаем по всем слотам в контейнере
        for (int i = 0; i < gridContainer.childCount; i++)
        {
            // Получаем скрипт слота
            UnitSlotUI slot = gridContainer.GetChild(i).GetComponent<UnitSlotUI>();

            // Если номер слота совпадает с selectedIndex -> Включаем рамку
            // Иначе -> Выключаем
            if (slot != null)
            {
                bool isSelected = (i == selectedIndex);
                slot.SetSelected(isSelected);
            }
        }
    }
}