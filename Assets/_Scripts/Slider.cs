using UnityEngine;
using UnityEngine.UI;

public class PixelSlider : MonoBehaviour
{
    [Header("Налаштування Аудіо")]
    public AudioSource audioSource; // Сюди тягни або Музику, або Звуки
    public Image backgroundDisplay;
    public Sprite[] frames;

    [Header("УНІКАЛЬНИЙ КЛЮЧ (MusicVol або SoundVol)")]
    public string saveKey = "MusicVolume";

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
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Примусово ставимо звук при старті (на випадок, якщо панель прихована)
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

        if (frames != null && frames.Length > 0 && backgroundDisplay != null)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt(value * (frames.Length - 1)), 0, frames.Length - 1);
            backgroundDisplay.sprite = frames[index];
        }
    }
}   