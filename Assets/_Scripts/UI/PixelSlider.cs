using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PixelSlider : MonoBehaviour
{
    public AudioSource audioSource;
    public string saveKey = "MusicVolume";
    public TMP_Text percentText;

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        SyncSlider();
    }

    void Start()
    {
        if (slider != null)
        {
            slider.wholeNumbers = false;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        ApplyStoredVolume();
        SyncSlider();
    }

    private void ApplyStoredVolume()
    {
        float savedValue = PlayerPrefs.GetFloat(saveKey, 1f);
        if (audioSource != null)
            audioSource.volume = savedValue;
    }

    public void SyncSlider()
    {
        float savedValue = PlayerPrefs.GetFloat(saveKey, 1f);
        if (slider != null)
            slider.value = savedValue;

        UpdateSliderVisuals(savedValue);
    }

    public void OnSliderChanged(float value)
    {
        PlayerPrefs.SetFloat(saveKey, value);
        PlayerPrefs.Save();
        UpdateSliderVisuals(value);
    }

    private void UpdateSliderVisuals(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
