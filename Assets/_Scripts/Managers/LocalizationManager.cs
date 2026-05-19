using System;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public LocalizationData english;
    public LocalizationData ukrainian;

    public static event Action OnLanguageChanged;

    private bool _isUkrainian;
    public bool IsUkrainian => _isUkrainian;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _isUkrainian = PlayerPrefs.GetInt("IsUkrainian", 0) == 1;
    }

    public void SetUkrainian(bool value)
    {
        _isUkrainian = value;
        PlayerPrefs.SetInt("IsUkrainian", value ? 1 : 0);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }

public string Get(string key)
    {
        var data = _isUkrainian ? ukrainian : english;
        return data != null ? data.Get(key) : $"[{key}]";
    }
}
