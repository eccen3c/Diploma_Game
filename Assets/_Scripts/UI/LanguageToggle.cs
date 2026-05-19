using UnityEngine;
using UnityEngine.UI;

public class LanguageToggle : MonoBehaviour
{
    public Sprite flagEN;
    public Sprite flagUA;

    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(Toggle);
        Refresh();
    }

    void Toggle()
    {
        LocalizationManager.Instance.SetUkrainian(!LocalizationManager.Instance.IsUkrainian);
        Refresh();
    }

    void Refresh()
    {
        _image.sprite = LocalizationManager.Instance.IsUkrainian ? flagUA : flagEN;
    }
}
