using UnityEngine;

public class ShopInput : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }

    [Header("Settings")]
    public PlayerID playerID;       // �������� � ����������: P1 ��� P2
    public Transform gridContainer; // ������ �� Shop_P1_Left ��� Shop_P2_Right

    [Header("State")]
    public int selectedIndex = 0;   // ����� ���� ������ ������ (0, 1, 2...)
    private int maxIndex;           // ������� ����� ������� ������

    // ������ �� ��������, ����� �����, ������� ������ ��������
    private ShopManager shopManager;

    void Start()
    {
        shopManager = FindObjectOfType<ShopManager>();

        // ���� �������, ����� ShopManager ����� ��������� ������
        // �� ����� ������ ����� ���������� �����, ���� ShopManager �������� � Awake
        // ��� ���������� ������� ������ � ������ ����� Update
    }

    void Update()
    {
        // 1. ������, ������� � ��� ������ ������ (����� �� ���� � �������)
        // ���� ShopManager ��� �� ����������� ��� ������ ���� - ������ �� ������
        if (shopManager == null || shopManager.allUnits == null) return;
        if (Time.timeScale == 0) return;
        maxIndex = shopManager.allUnits.Count - 1;

        // 2. ������ ����������
        if (playerID == PlayerID.Player1)
        {
            if (Input.GetKeyDown(KeyCode.D)) MoveSelection(1);       // ������
            if (Input.GetKeyDown(KeyCode.A)) MoveSelection(-1);      // �����
            if (Input.GetKeyDown(KeyCode.W)) MoveSelection(-8);      // ����� (��� -8)
            if (Input.GetKeyDown(KeyCode.S)) MoveSelection(8);       // ���� (��� +8)
        }
        else // Player 2
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSelection(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSelection(-1);
            if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSelection(-8);
            if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSelection(8);
        }

        // 3. ��������� ������ (�������� �����)
        UpdateVisuals();
    }

    void MoveSelection(int change)
    {
        int newIndex = selectedIndex + change;

        // ��������, ����� �� ����� �� �������
        if (newIndex >= 0 && newIndex <= maxIndex)
        {
            selectedIndex = newIndex;
        }
    }

    void UpdateVisuals()
    {
        // ��������� �� ���� ������ � ����������
        for (int i = 0; i < gridContainer.childCount; i++)
        {
            // �������� ������ �����
            UnitSlotUI slot = gridContainer.GetChild(i).GetComponent<UnitSlotUI>();

            // ���� ����� ����� ��������� � selectedIndex -> �������� �����
            // ����� -> ���������
            if (slot != null)
            {
                bool isSelected = (i == selectedIndex);
                slot.SetSelected(isSelected);
            }
        }
    }
}