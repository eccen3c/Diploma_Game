using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class UnitSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public GameObject selectionFrame;

    [Header("Data")]
    public UnitData unitData;

    // true = —ин≥ (√равець 1), false = „ервон≥ (√равець 2)
    private bool isBlueSlot;

    public void SetupSlot(UnitData data, bool isBlue = true)
    {
        unitData = data;
        isBlueSlot = isBlue;

        Sprite icon = isBlue ? unitData.iconBlue : unitData.iconRed;
        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.color = Color.white;
        }

        float flipX = isBlue ? 1f : -1f;
        iconImage.rectTransform.localScale = new Vector3(flipX, 1f, 1f);

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnSlotClick);
        }

        if (selectionFrame != null) selectionFrame.SetActive(false);
    }

    public void OnSlotClick()
    {
        if (unitData == null) return;

        // ѕ≈–≈¬≤– ј ƒЋя ќЌЋј…Ќ”:  ожен гравець може кл≥кати “≤Ћ№ » на своњ слоти!
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // якщо € ћайстер (—творювач к≥мнати), € можу кл≥кати т≥льки на —ин≥ слоти
            if (PhotonNetwork.IsMasterClient && !isBlueSlot) return;
            // якщо €  л≥Їнт (ѕриЇднавс€), € можу кл≥кати т≥льки на „ервон≥ слоти
            if (!PhotonNetwork.IsMasterClient && isBlueSlot) return;
        }

        int slotIndex = transform.GetSiblingIndex();
        int playerNum = isBlueSlot ? 1 : 2;

        // якщо ми в мереж≥ Ч синхрон≥зуЇмо кл≥к на ≥ншому ѕ 
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            GameLoopManager.instance.photonView.RPC("RPC_SelectUnit", RpcTarget.All, playerNum, slotIndex);
        }
        else
        {
            // якщо тестуЇш сам локально
            GameLoopManager.instance.LocalSelect(playerNum, slotIndex);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(isSelected);
        }
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