using UnityEngine;
using TMPro;

public class HowToPlayPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    void OnEnable()
    {
        Refresh();
        LocalizationManager.OnLanguageChanged += Refresh;
    }

    void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= Refresh;
    }

    void Refresh()
    {
        var loc = LocalizationManager.Instance;
        if (loc == null) return;

        titleText.text = loc.Get("htp_title");
        contentText.text = loc.Get("htp_content");
    }
}
