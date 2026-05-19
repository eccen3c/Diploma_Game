using UnityEngine;

public class VolumeInitializer : MonoBehaviour
{
    public AudioSource musicSource;
    public string saveKey = "MusicVolume";

    // Міняємо Start на Awake
    void Awake()
    {
        // Отримуємо збережене значення
        float savedValue = PlayerPrefs.GetFloat(saveKey, 1f);

        if (musicSource != null)
        {
            // Встановлюємо гучність ПЕРЕД тим, як музика почне грати
            musicSource.volume = savedValue;
        }
    }
}