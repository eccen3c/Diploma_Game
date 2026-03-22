using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LanguageSpritesManager : MonoBehaviour
{
    public bool isUkrainian = false;

    [Header("Слайдери звуків бою")]
    public Slider engCombatSlider;
    public Slider ukrCombatSlider;

    [Header("Налаштування роздільної здатності")]
    public TMP_Dropdown engResDropdown;
    public TMP_Dropdown ukrResDropdown;
    private Resolution[] resolutions;

    [Header("Синхронізація чекбоксів")]
    public Toggle engFullscreenToggle;
    public Toggle ukrFullscreenToggle;

    [Header("Панелі налаштувань (Layouts)")]
    public GameObject engLayout;
    public GameObject ukrLayout;

    [Header("Синхронізація слайдерів гучності")]
    public Slider engVolumeSlider;
    public Slider ukrVolumeSlider;

    [Header("Центральний перемикач (плашка)")]
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

    [Header("Режими гри (Single)")]
    public Image easyButtonImage;
    public Sprite easyEng, easyUkr;
    public Image mediumButtonImage;
    public Sprite mediumEng, mediumUkr;
    public Image hardButtonImage;
    public Sprite hardEng, hardUkr;

    [Header("Мультиплеєр (Multi)")]
    public Image player2ButtonImage;
    public Sprite player2Eng, player2Ukr;
    public Image onlineButtonImage;
    public Sprite onlineEng, onlineUkr;

    void Start()
    {
        SetupResolutions();

        // Завантажуємо збережені значення, щоб не скидалося на 1 щоразу
        float savedVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedCombat = PlayerPrefs.GetFloat("CombatVolume", 1f);

        if (engVolumeSlider != null) engVolumeSlider.value = savedVol;
        if (ukrVolumeSlider != null) ukrVolumeSlider.value = savedVol;
        if (engCombatSlider != null) engCombatSlider.value = savedCombat;
        if (ukrCombatSlider != null) ukrCombatSlider.value = savedCombat;

        if (engLayout != null) engLayout.SetActive(!isUkrainian);
        if (ukrLayout != null) ukrLayout.SetActive(isUkrainian);
        UpdateButtonsUI();
    }

    public void SaveMusicVolume(float val)
    {
        PlayerPrefs.SetFloat("MusicVolume", val);
        PlayerPrefs.Save();

        // Синхронізуємо цифру
        if (isUkrainian && engVolumeSlider != null) engVolumeSlider.value = val;
        else if (!isUkrainian && ukrVolumeSlider != null) ukrVolumeSlider.value = val;
    }

    public void SaveCombatVolume(float val)
    {
        PlayerPrefs.SetFloat("CombatVolume", val);
        PlayerPrefs.Save();

        // Синхронізуємо цифру
        if (isUkrainian && engCombatSlider != null) engCombatSlider.value = val;
        else if (!isUkrainian && ukrCombatSlider != null) ukrCombatSlider.value = val;
    }

    public void SwitchLanguage()
    {
        isUkrainian = !isUkrainian;

        if (engLayout != null) engLayout.SetActive(!isUkrainian);
        if (ukrLayout != null) ukrLayout.SetActive(isUkrainian);

        if (languageDisplayImage != null)
            languageDisplayImage.sprite = isUkrainian ? langLabelUkr : langLabelEng;

        // --- СИНХРОНІЗАЦІЯ ТА ОНОВЛЕННЯ ВІЗУАЛУ ---
        if (isUkrainian)
        {
            // Музика
            if (ukrVolumeSlider != null && engVolumeSlider != null)
            {
                ukrVolumeSlider.value = engVolumeSlider.value;
                ukrVolumeSlider.GetComponent<PixelSlider>()?.OnSliderChanged(ukrVolumeSlider.value);
            }
            // Звуки
            if (ukrCombatSlider != null && engCombatSlider != null)
            {
                ukrCombatSlider.value = engCombatSlider.value;
                ukrCombatSlider.GetComponent<PixelSlider>()?.OnSliderChanged(ukrCombatSlider.value);
            }
        }
        else
        {
            // Музика
            if (engVolumeSlider != null && ukrVolumeSlider != null)
            {
                engVolumeSlider.value = ukrVolumeSlider.value;
                engVolumeSlider.GetComponent<PixelSlider>()?.OnSliderChanged(engVolumeSlider.value);
            }
            // Звуки
            if (engCombatSlider != null && ukrCombatSlider != null)
            {
                engCombatSlider.value = ukrCombatSlider.value;
                engCombatSlider.GetComponent<PixelSlider>()?.OnSliderChanged(engCombatSlider.value);
            }
        }

        // Синхронізація фулскріну
        if (isUkrainian) ukrFullscreenToggle.isOn = engFullscreenToggle.isOn;
        else engFullscreenToggle.isOn = ukrFullscreenToggle.isOn;

        UpdateButtonsUI();
    }

    // Решта твоїх методів (SetupResolutions, UpdateButtonsUI тощо) залишаються без змін
    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        engResDropdown.ClearOptions();
        ukrResDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        engResDropdown.AddOptions(options);
        ukrResDropdown.AddOptions(options);
        engResDropdown.value = currentResolutionIndex;
        ukrResDropdown.value = currentResolutionIndex;
        engResDropdown.RefreshShownValue();
        ukrResDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex >= resolutions.Length) return;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        engResDropdown.value = resolutionIndex;
        ukrResDropdown.value = resolutionIndex;
    }

    private void UpdateButtonsUI()
    {
        if (isUkrainian)
        {
            if (playButtonImage) playButtonImage.sprite = playUkr;
            if (settingsButtonImage) settingsButtonImage.sprite = settingsUkr;
            if (multiButtonImage) multiButtonImage.sprite = multiUkr;
            if (easyButtonImage) easyButtonImage.sprite = easyUkr;
            if (mediumButtonImage) mediumButtonImage.sprite = mediumUkr;
            if (hardButtonImage) hardButtonImage.sprite = hardUkr;
            if (player2ButtonImage) player2ButtonImage.sprite = player2Ukr;
            if (onlineButtonImage) onlineButtonImage.sprite = onlineUkr;
        }
        else
        {
            if (playButtonImage) playButtonImage.sprite = playEng;
            if (settingsButtonImage) settingsButtonImage.sprite = settingsEng;
            if (multiButtonImage) multiButtonImage.sprite = multiEng;
            if (easyButtonImage) easyButtonImage.sprite = easyEng;
            if (mediumButtonImage) mediumButtonImage.sprite = mediumEng;
            if (hardButtonImage) hardButtonImage.sprite = hardEng;
            if (player2ButtonImage) player2ButtonImage.sprite = player2Eng;
            if (onlineButtonImage) onlineButtonImage.sprite = onlineEng;
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Повний екран: " + isFullscreen);
    }
}