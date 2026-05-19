using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Language")]
    public Button buttonEN;
    public Button buttonUA;

    void Start()
    {
        buttonEN.onClick.AddListener(SetEnglish);
        buttonUA.onClick.AddListener(SetUkrainian);
        RefreshLanguageButtons();
        LocalizationManager.OnLanguageChanged += RefreshLanguageButtons;
    }

    void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshLanguageButtons;
    }

    void SetEnglish()
    {
        LocalizationManager.Instance.SetUkrainian(false);
    }

    void SetUkrainian()
    {
        LocalizationManager.Instance.SetUkrainian(true);
    }

    void RefreshLanguageButtons()
    {
        bool isUA = LocalizationManager.Instance.IsUkrainian;
        buttonEN.interactable = isUA;
        buttonUA.interactable = !isUA;
    }
}
