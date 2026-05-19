using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public GameObject selectionFrame;

    [Header("Data")]
    public UnitData unitData;

    public void SetupSlot(UnitData data, bool isBlue = true)
    {
        unitData = data;
        Sprite icon = isBlue ? unitData.iconBlue : unitData.iconRed;
        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.color = Color.white;
        }
        float flipX = isBlue ? 1f : -1f;
        iconImage.rectTransform.localScale = new Vector3(flipX, 1f, 1f);
    }

    public void SetSelected(bool isSelected)
    {
        selectionFrame.SetActive(isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitData != null && TooltipUI.Instance != null)
            TooltipUI.Instance.Show(unitData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }
}
