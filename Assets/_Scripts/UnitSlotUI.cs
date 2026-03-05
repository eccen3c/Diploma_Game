using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // НУЖНО ДОБАВИТЬ ЭТУ БИБЛИОТЕКУ!

// Добавляем интерфейсы через запятую
public class UnitSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public GameObject selectionFrame;

    [Header("Data")]
    public UnitData unitData;

    public void SetupSlot(UnitData data)
    {
        unitData = data;
        if (unitData.icon != null)
        {
            iconImage.sprite = unitData.icon;
            iconImage.color = Color.white;
        }
    }

    public void SetSelected(bool isSelected)
    {
        selectionFrame.SetActive(isSelected);
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ МЫШКИ ---

    // Когда мышка зашла на слот
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitData != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Show(unitData);
        }
    }

    // Когда мышка ушла со слота
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.Hide();
        }
    }
}