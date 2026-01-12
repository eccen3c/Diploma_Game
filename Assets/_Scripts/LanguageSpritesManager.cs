using UnityEngine;
using UnityEngine.UI; // Працюємо з картинками

public class LanguageSpritesManager : MonoBehaviour
{
    public bool isUkrainian = false;

    [Header("Кнопка Play")]
    public Image playButtonImage;
    public Sprite playEng;
    public Sprite playUkr;

    [Header("Кнопка Settings")]
    public Image settingsButtonImage;
    public Sprite settingsEng;
    public Sprite settingsUkr;

    [Header("Кнопка Multiplay")]
    public Image multiButtonImage;
    public Sprite multiEng;
    public Sprite multiUkr;

    // Додай такі ж пари для інших кнопок (Exit, Multiplay тощо)

    public void SwitchLanguage()
    {
        isUkrainian = !isUkrainian;

        if (isUkrainian)
        {
            playButtonImage.sprite = playUkr;
            settingsButtonImage.sprite = settingsUkr;
            multiButtonImage.sprite = multiUkr;
        }
        else
        {
            playButtonImage.sprite = playEng;
            multiButtonImage.sprite = multiEng;
            settingsButtonImage.sprite = settingsEng;
        }
    }
}