using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("������")]
    public string unitName;
    public string unitNameUk;
    public Sprite iconBlue;
    public Sprite iconRed;

    [Header("������� (������ ��� ������)")]
    public GameObject prefab_Player; // ���� ������� ������
    public GameObject prefab_Enemy;  // ���� ������� ��������

    [Header("���������")]
    public int cost;
    public int incomeBonus;
    public int supplyCost;

    [Header("������ ��������������")]
    public float hp;
    public float damage;
    public float moveSpeed;
    public float attackRange;
    public float visionRange;
    public float attackSpeed;
}