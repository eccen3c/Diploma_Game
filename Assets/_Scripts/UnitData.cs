using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Визуал")]
    public string unitName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Префабы (Разные для сторон)")]
    public GameObject prefab_Player; // Сюда кидаешь Синего
    public GameObject prefab_Enemy;  // Сюда кидаешь Красного

    [Header("Экономика")]
    public int cost;
    public int incomeBonus;
    public int supplyCost;

    [Header("Боевые Характеристики")]
    public float hp;
    public float damage;
    public float moveSpeed;
    public float attackRange;
    public float visionRange;
    public float attackSpeed;
}