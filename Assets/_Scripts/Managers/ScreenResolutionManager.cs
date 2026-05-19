using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScreenResolutionManager : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown dropdown;

    [Header("Fullscreen")]
    public Button fullscreenButton;
    public Sprite spriteChecked;
    public Sprite spriteUnchecked;

    private Image _fullscreenImage;
    private Resolution[] resolutions;

    void Start()
    {
        resolutions = Screen.resolutions;
        if (resolutions == null || resolutions.Length == 0) return;

        dropdown.ClearOptions();
        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentIndex = i;
        }

        dropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("SelectedRes", currentIndex);
        if (savedIndex >= resolutions.Length) savedIndex = currentIndex;

        dropdown.value = savedIndex;
        dropdown.onValueChanged.AddListener(SetResolution);
        ApplyResolution(savedIndex);

        if (fullscreenButton != null)
        {
            _fullscreenImage = fullscreenButton.GetComponent<Image>();
            fullscreenButton.onClick.AddListener(ToggleFullscreen);
        }

        UpdateFullscreenSprite();
    }

    void SetResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("SelectedRes", index);
        PlayerPrefs.Save();
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateFullscreenSprite();
    }

    void UpdateFullscreenSprite()
    {
        if (_fullscreenImage == null) return;
        _fullscreenImage.sprite = Screen.fullScreen ? spriteChecked : spriteUnchecked;
    }

    void ApplyResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
