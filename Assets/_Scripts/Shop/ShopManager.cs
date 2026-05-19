using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ����� ��� ���������� (�� �������)

public class ShopManager : MonoBehaviour
{
    [Header("Containers")]
    public Transform shopContainerP1; // ������ �� Shop_P1_Left
    public Transform shopContainerP2; // ������ �� Shop_P2_Right

    [Header("Data")]
    public List<UnitData> allUnits;   // ������ ���� ����������� ������

    private void Start()
    {
        LoadUnits();
        InitializeShop(shopContainerP1, isBlue: true);
        InitializeShop(shopContainerP2, isBlue: false);
    }

    void LoadUnits()
    {
        // 1. ��������: ���� �� ��� ������� ������ ������ � ���������� (������ �� ����)
        if (allUnits != null && allUnits.Count > 0)
        {
            Debug.Log("���������� ������ ������ �� ���������� (���������� ���������).");
            return; // ������� �� ������. ������ ��� �� ������.
        }

        // 2. ������: ���� ������ ����, ������ ��������� (��� ���� ������)
        Debug.Log("������ ����. ��������� �� �� ����� Resources �������������.");

        UnitData[] loaded = Resources.LoadAll<UnitData>("Units");

        // ���� ������ ����-���������� �� ���� � �������������� ������ ����.
        // allUnits = loaded.OrderBy(u => u.cost).ToList(); 

        // � ���� ������ ������ ��� ������ (��� �� �������� ���� ������)
        allUnits = loaded.ToList();

        Debug.Log($"����-��������� ������: {allUnits.Count}");
    }

    void InitializeShop(Transform container, bool isBlue = true)
    {
        // ��������� �� ���� ������� (UnitSlot) ������ ����������
        for (int i = 0; i < container.childCount; i++)
        {
            // ����� ���� ��� ������� i
            UnitSlotUI slot = container.GetChild(i).GetComponent<UnitSlotUI>();

            // ���� � ��� ���� ���� ��� ����� �����
            if (i < allUnits.Count)
            {
                slot.SetupSlot(allUnits[i], isBlue);
            }
            else
            {
                // ���� ������ ������, ��� ������ (��������, ����� 4 �����, � ������ 16)
                // ����� ��������� ������ ����� ��� �������� �������
                slot.gameObject.SetActive(false); // ��������������, ���� ������ ������ ������
            }
        }
    }
}