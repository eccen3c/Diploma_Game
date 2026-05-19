using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LanguageSpritesManager : MonoBehaviour
{
    public bool isUkrainian = false;

    [Header("Кнопки Паузи")]
    public Image resumeButtonImage;
    public Sprite resumeEng, resumeUkr;
    public Image menuButtonImage;
    public Sprite menuEng, menuUkr;
    public Image settingsPauseButtonImage;
    public Sprite settingsPauseEng, settingsPauseUkr;

    [Header("Кнопки Game Over")]
    public Image gameOverRestartImage;
    public Sprite restartEng, restartUkr;
    public Image gameOverMenuImage;
    public Sprite menuOVerEng, menuOVerUkr;

    [Header("Налаштування відео")]
    public TMP_Dropdown engResDropdown;
    public TMP_Dropdown ukrResDropdown;
    private Resolution[] resolutions;
    public Toggle engFullscreenToggle;
    public Toggle ukrFullscreenToggle;

    [Header("Панелі та Лейбли")]
    public GameObject engLayout;
    public GameObject ukrLayout;
    public Image languageDisplayImage;
    public Sprite langLabelEng;
    public Sprite langLabelUkr;

    [Header("Головні кнопки")]
    public Image playButtonImage;
    public Sprite playEng, playUkr;
    public Image settingsButtonImage;
    public Sprite settingsEng, settingsUkr;
    public Image multiButtonImage;
    public Sprite multiEng, multiUkr;

    [Header("Режими гри")]
    public Image easyButtonImage;
    public Sprite easyEng, easyUkr;
    public Image mediumButtonImage;
    public Sprite mediumEng, mediumUkr;
    public Image hardButtonImage;
    public Sprite hardEng, hardUkr;

    [Header("Мультиплеєр")]
    public Image player2ButtonImage;
    public Sprite player2Eng, player2Ukr;
    public Image onlineButtonImage;
    public Sprite onlineEng, onlineUkr;

    void Awake()
    {
        // Це вирішує твою проблему: зчитуємо мову, як тільки скрипт з'явився
        RefreshLanguage();
    }

    void OnEnable()
    {
        RefreshLanguage();
    }

    void Start()
    {
        SetupResolutions();
        RefreshLanguage();

        bool savedFS = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        Screen.fullScreen = savedFS;
        if (engFullscreenToggle != null) engFullscreenToggle.isOn = savedFS;
        if (ukrFullscreenToggle != null) ukrFullscreenToggle.isOn = savedFS;
    }

    public void RefreshLanguage()
    {
        isUkrainian = PlayerPrefs.GetInt("IsUkrainian", 0) == 1;

        if (engLayout != null) engLayout.SetActive(!isUkrainian);
        if (ukrLayout != null) ukrLayout.SetActive(isUkrainian);

        // Зчитуємо саме з PlayerPrefs, це найнадійніше джерело істини
        bool savedFS = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;

        // Важливо: спочатку вимикаємо слухач подій (якщо він є), 
        // або просто присвоюємо значення, яке не має викликати SetFullscreen знову.
        if (engFullscreenToggle != null) engFullscreenToggle.isOn = savedFS;
        if (ukrFullscreenToggle != null) ukrFullscreenToggle.isOn = savedFS;

        if (languageDisplayImage != null)
            languageDisplayImage.sprite = isUkrainian ? langLabelUkr : langLabelEng;

        UpdateButtonsUI();
    }

    public void SwitchLanguage()
    {
        isUkrainian = !isUkrainian;
        PlayerPrefs.SetInt("IsUkrainian", isUkrainian ? 1 : 0);
        PlayerPrefs.Save();
        RefreshLanguage();
    }

    public void UpdateButtonsUI()
    {
        if (isUkrainian)
        {
            // УКРАЇНСЬКІ СПРАЙТИ
            SetS(playButtonImage, playUkr);
            SetS(settingsButtonImage, settingsUkr);
            SetS(multiButtonImage, multiUkr);
            SetS(easyButtonImage, easyUkr);
            SetS(mediumButtonImage, mediumUkr);
            SetS(hardButtonImage, hardUkr);
            SetS(player2ButtonImage, player2Ukr);
            SetS(onlineButtonImage, onlineUkr);

            SetS(resumeButtonImage, resumeUkr);
            SetS(menuButtonImage, menuUkr);
            SetS(settingsPauseButtonImage, settingsPauseUkr);

            SetS(gameOverRestartImage, restartUkr);
            SetS(gameOverMenuImage, menuOVerUkr);
        }
        else
        {
            // АНГЛІЙСЬКІ СПРАЙТИ
            SetS(playButtonImage, playEng);
            SetS(settingsButtonImage, settingsEng);
            SetS(multiButtonImage, multiEng);
            SetS(easyButtonImage, easyEng);
            SetS(mediumButtonImage, mediumEng);
            SetS(hardButtonImage, hardEng);
            SetS(player2ButtonImage, player2Eng);
            SetS(onlineButtonImage, onlineEng);

            SetS(resumeButtonImage, resumeEng);
            SetS(menuButtonImage, menuEng);
            SetS(settingsPauseButtonImage, settingsPauseEng);

            SetS(gameOverRestartImage, restartEng);
            SetS(gameOverMenuImage, menuOVerEng);
        }
    }

    private void SetS(Image img, Sprite sp)
    {
        if (img != null && sp != null)
            img.sprite = sp;
    }

    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        if (engResDropdown == null || ukrResDropdown == null) return;

        engResDropdown.ClearOptions();
        ukrResDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResIndex = i;
        }

        engResDropdown.AddOptions(options);
        ukrResDropdown.AddOptions(options);

        int savedRes = PlayerPrefs.GetInt("SelectedRes", currentResIndex);
        if (savedRes < resolutions.Length)
        {
            engResDropdown.value = savedRes;
            ukrResDropdown.value = savedRes;
        }

        engResDropdown.RefreshShownValue();
        ukrResDropdown.RefreshShownValue();
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || index >= resolutions.Length) return;
        Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreen);
        PlayerPrefs.SetInt("SelectedRes", index);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        // Спочатку оновлюємо глобальний стан
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        // Синхронізуємо обидва чекбокси, щоб вони завжди були однакові
        if (engFullscreenToggle != null) engFullscreenToggle.isOn = isFullscreen;
        if (ukrFullscreenToggle != null) ukrFullscreenToggle.isOn = isFullscreen;
    }
}