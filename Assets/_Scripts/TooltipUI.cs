using UnityEngine;
using TMPro; // Обязательно для текста

public class TooltipUI : MonoBehaviour
{
    // Делаем Синглтон (статик), чтобы любой слот мог легко найти этот скрипт
    public static TooltipUI Instance;

    [Header("UI Links")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descText;

    [Header("Settings")]
    public Vector2 offset = new Vector2(15, -15); // Сдвиг от курсора вправо-вниз

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // Скрываем при старте
    }

    private void Update()
    {
        // Панель следует за мышкой + сдвиг
        transform.position = Input.mousePosition + (Vector3)offset;
    }

    public void Show(UnitData unit)
    {
        // Включаем панель
        gameObject.SetActive(true);

        // Заполняем тексты
        nameText.text = unit.unitName;
        costText.text = $"Cost: <color=yellow>{unit.cost}</color>  Income: <color=green>+{unit.incomeBonus}</color>";

        // Формируем список статов
        statsText.text =
            $"HP: {unit.hp}\n" +
            $"Dmg: {unit.damage}\n" +
            $"Spd: {unit.attackSpeed}s";

        // Описание (если есть)
        descText.text = unit.description;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}