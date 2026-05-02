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
        if (sfxSource != null)
        {
            sfxSource.volume = PlayerPrefs.GetFloat("SoundVol", 1f);
        }
    }

    public void PlayHitSound()
    {
        if (sfxSource == null || sfxSource.volume <= 0) return;
  
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