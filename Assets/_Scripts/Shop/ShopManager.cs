using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// Додаємо Photon
using Photon.Pun;

public class ShopManager : MonoBehaviour
{
    [Header("Containers")]
    public Transform shopContainerP1; // Лівий магазин (Сині)
    public Transform shopContainerP2; // Правий магазин (Червоні)

    [Header("Data")]
    public List<UnitData> allUnits;

    private void Start()
    {
        LoadUnits();
        InitializeShop(shopContainerP1, isBlue: true);
        InitializeShop(shopContainerP2, isBlue: false);

        // НАЛАШТУВАННЯ ДЛЯ ОНЛАЙНУ:
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Я Синій гравець (Хост) -> вимикаю для себе інтерфейс Червоного
                DisableShopContainer(shopContainerP2);
            }
            else
            {
                // Я Червоний гравець (Клієнт) -> вимикаю для себе інтерфейс Синього
                DisableShopContainer(shopContainerP1);
            }
        }
        // Якщо НЕ в мережі — обидва магазини залишаються активними (твій старий режим 1 на 1)
    }

    void LoadUnits()
    {
        if (allUnits != null && allUnits.Count > 0)
        {
            Debug.Log("Слони завантажені з інспектора.");
            return;
        }

        Debug.Log("Завантаження юнітів з Resources/Units...");
        UnitData[] loaded = Resources.LoadAll<UnitData>("Units");
        allUnits = loaded.ToList();
    }

    void InitializeShop(Transform container, bool isBlue = true)
    {
        for (int i = 0; i < container.childCount; i++)
        {
            UnitSlotUI slot = container.GetChild(i).GetComponent<UnitSlotUI>();

            if (slot != null && i < allUnits.Count)
            {
                slot.SetupSlot(allUnits[i], isBlue);
            }
            else if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    // Функція, яка робить кнопки чужого магазину неактивними
    void DisableShopContainer(Transform container)
    {
        // Можна або повністю сховати панель:
        // container.gameObject.SetActive(false);

        // Або, що значно красивіше — вимкнути кнопки, щоб гравець бачив інтерфейс ворога, але не міг нічого купити:
        UnityEngine.UI.Button[] buttons = container.GetComponentsInChildren<UnityEngine.UI.Button>();
        foreach (var btn in buttons)
        {
            btn.interactable = false; // Робимо кнопку сірою та неактивною
        }
    }
}