using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip screamClip;
    public AudioClip hitClip;
    public AudioClip arrowClip;
    public AudioClip fireballClip;

    private float _lastHitTime = -10f;
    private const float HIT_COOLDOWN = 0.25f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        ApplySavedVolume();
    }

    public void ApplySavedVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayHitSound()
    {
        if (sfxSource == null || hitClip == null) return;
        if (Time.time - _lastHitTime < HIT_COOLDOWN) return;
        _lastHitTime = Time.time;
        sfxSource.PlayOneShot(hitClip);
    }

    public void PlayScream()
    {
        if (sfxSource != null && screamClip != null)
            sfxSource.PlayOneShot(screamClip);
    }

    public void PlayArrow()
    {
        if (sfxSource != null && arrowClip != null)
            sfxSource.PlayOneShot(arrowClip);
    }

    public void PlayFireball()
    {
        if (sfxSource != null && fireballClip != null)
            sfxSource.PlayOneShot(fireballClip);
    }
}
