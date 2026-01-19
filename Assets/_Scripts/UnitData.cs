using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Визуал")]
    public string unitName;        // Имя (например, "Soldier")
    public Sprite icon;            // Иконка для магазина
    public GameObject unitPrefab;  // Префаб, который появляется на поле
    [TextArea] public string description; // Описание для тултипа

    [Header("Экономика")]
    public int cost;               // Цена покупки (Gold)
    public int incomeBonus;        // Сколько добавляет к доходу (+Gold)
    public int supplyCost;         // Сколько занимает места (Supply)

    [Header("Характеристики")]
    public float hp;               // Здоровье
    public float damage;           // Урон
    public float attackSpeed;      // Скорость атаки (пауза между ударами)
    public float moveSpeed;        // Скорость бега
    public float attackRange;      // Дальность (1 = ближний, 5+ = лучник)
}