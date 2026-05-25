using UnityEngine;
using Photon.Pun; // Додаємо Фотон, щоб знати, хто є хто в мережі

public class ShopInput : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }

    [Header("Settings")]
    public PlayerID playerID;       // Визначаємо в інспекторі: P1 або P2
    public Transform gridContainer; // Посилання на Сcontainer зі слотами

    [Header("State")]
    public int selectedIndex = 0;   // Індекс, який зараз вибрано
    private int maxIndex;           // Максимальний індекс

    private ShopManager shopManager;

    void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();
    }

    void Update()
    {
        if (shopManager == null || shopManager.allUnits == null) return;
        if (Time.timeScale == 0) return;
        maxIndex = shopManager.allUnits.Count - 1;

        // --- МЕРЕЖЕВИЙ ЗАХИСТ ---
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Якщо я Майстер (ПК), я можу натискати ТІЛЬКИ кнопки для Player1 (WASD)
            if (PhotonNetwork.IsMasterClient && playerID == PlayerID.Player2) return;

            // Якщо я Клієнт (Ноутбук), я можу натискати ТІЛЬКИ кнопки для Player2 (Стрілочки)
            if (!PhotonNetwork.IsMasterClient && playerID == PlayerID.Player1) return;
        }

        // 2. Обробка введення
        if (playerID == PlayerID.Player1)
        {
            if (Input.GetKeyDown(KeyCode.D)) MoveSelection(1);       // Вправо
            if (Input.GetKeyDown(KeyCode.A)) MoveSelection(-1);      // Вліво
            if (Input.GetKeyDown(KeyCode.W)) MoveSelection(-8);      // Вгору (крок -8)
            if (Input.GetKeyDown(KeyCode.S)) MoveSelection(8);       // Вниз (крок +8)
        }
        else // Player 2
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSelection(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSelection(-1);
            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSelection(-8);
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSelection(8);
        }

        // 3. Візуальне оновлення рамки
        UpdateVisuals();
    }

    void MoveSelection(int change)
    {
        int newIndex = selectedIndex + change;

        if (newIndex >= 0 && newIndex <= maxIndex)
        {
            // Якщо ми в онлайні, замість простої зміни змінної — шлемо запит через RPC
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                int pNum = (playerID == PlayerID.Player1) ? 1 : 2;

                // Шлемо команду всім пристроям: "Оновити рамку для такого-то гравця на такий-то індекс"
                GameLoopManager.instance.photonView.RPC("RPC_SelectUnit", RpcTarget.All, pNum, newIndex);
            }
            else
            {
                // Якщо тестуєш гру сам із собою локально (без Фотону)
                selectedIndex = newIndex;
            }
        }
    }

    void UpdateVisuals()
    {
        if (gridContainer == null) return;

        for (int i = 0; i < gridContainer.childCount; i++)
        {
            UnitSlotUI slot = gridContainer.GetChild(i).GetComponent<UnitSlotUI>();
            if (slot != null)
            {
                bool isSelected = (i == selectedIndex);
                slot.SetSelected(isSelected);
            }
        }
    }
}