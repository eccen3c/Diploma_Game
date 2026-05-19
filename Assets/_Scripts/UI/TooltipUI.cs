using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [Header("UI Links")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI economyText;
    public TextMeshProUGUI statsText;

    [Header("Settings")]
    public Vector2 offset = new Vector2(16, 16);
    public RectTransform panelRect;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 pivot = new Vector2(0f, 0f);

        float panelW = panelRect != null ? panelRect.rect.width : 220f;
        float panelH = panelRect != null ? panelRect.rect.height : 160f;

        if (mousePos.x + panelW + offset.x > Screen.width)  pivot.x = 1f;
        if (mousePos.y + panelH + offset.y > Screen.height) pivot.y = 1f;

        if (panelRect != null) panelRect.pivot = pivot;

        float ox = pivot.x == 0f ? offset.x : -offset.x;
        float oy = pivot.y == 0f ? offset.y : -offset.y;

        transform.position = new Vector3(mousePos.x + ox, mousePos.y + oy, 0f);
    }

    private UnitData _currentUnit;

    public void Show(UnitData unit)
    {
        Debug.Log("[TooltipUI] Show: " + unit.unitName);
        _currentUnit = unit;
        gameObject.SetActive(true);
        Refresh();
    }

    void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += Refresh;
    }

    void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= Refresh;
    }

    void Refresh()
    {
        if (_currentUnit == null) return;
        var loc = LocalizationManager.Instance;

        bool isUk = loc.IsUkrainian;
        nameText.text = (isUk && !string.IsNullOrEmpty(_currentUnit.unitNameUk))
            ? _currentUnit.unitNameUk
            : _currentUnit.unitName;

        economyText.text =
            $"{loc.Get("tooltip_cost")}: <color=yellow>{_currentUnit.cost}</color>    {loc.Get("tooltip_supply")}: <color=white>{_currentUnit.supplyCost}</color>\n" +
            $"{loc.Get("tooltip_income")}: <color=#44ff44>+{_currentUnit.incomeBonus}</color>";

        statsText.text =
            $"<color=yellow>{loc.Get("tooltip_hp")}:</color> {_currentUnit.hp}\n" +
            $"<color=yellow>{loc.Get("tooltip_damage")}:</color> {_currentUnit.damage}\n" +
            $"<color=yellow>{loc.Get("tooltip_atkspd")}:</color> {_currentUnit.attackSpeed}s\n" +
            $"<color=yellow>{loc.Get("tooltip_speed")}:</color> {_currentUnit.moveSpeed}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
