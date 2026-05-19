using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string key;

    private TMP_Text _text;

    void Awake() => _text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable() => LocalizationManager.OnLanguageChanged -= Refresh;

    void Refresh()
    {
        if (LocalizationManager.Instance != null)
            _text.text = LocalizationManager.Instance.Get(key);
    }
}
