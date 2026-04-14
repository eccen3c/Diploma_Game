using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Компоненти")]
    public AudioSource sfxSource;
    public AudioClip screamClip;
    public AudioClip hitClip;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Синхронізація зі слайдером (як у твоєму Pixel Slider)
        if (sfxSource != null)
        {
            // Використовуємо ключ SoundVol, який вказано у твоїх налаштуваннях
            sfxSource.volume = PlayerPrefs.GetFloat("SoundVol", 1f);
        }
    }

    public void PlayHitSound()
    {
        // Перевірка 1: чи не вимкнено звук у налаштуваннях
        if (sfxSource == null || sfxSource.volume <= 0) return;

        // ПЕРЕВІРКА 2: Головна фішка
        // .isPlaying повертає true, якщо AudioSource зараз щось грає
        // Ми запустимо новий звук ТІЛЬКИ якщо старий уже закінчився
        if (!sfxSource.isPlaying)
        {
            sfxSource.PlayOneShot(hitClip);
        }
    }

    public void PlayScream()
    {
        if (sfxSource != null && screamClip != null && sfxSource.volume > 0)
        {
            sfxSource.PlayOneShot(screamClip);
        }
    }
}   