using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationData", menuName = "Localization/Language Data")]
public class LocalizationData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string key;
        [TextArea] public string value;
    }

    public List<Entry> entries = new List<Entry>();

    public string Get(string key)
    {
        foreach (var entry in entries)
            if (entry.key == key) return entry.value;
        return $"[{key}]";
    }
}
