using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScreenResolutionManager : MonoBehaviour
{
    public TMP_Dropdown engDropdown;
    public TMP_Dropdown ukrDropdown;
    private Resolution[] resolutions;

    void Start()
    {
        // Отримуємо доступні роздільні здатності
        resolutions = Screen.resolutions;

        // Перевіряємо, чи масив не порожній
        if (resolutions == null || resolutions.Length == 0)
        {
            Debug.LogError("Не вдалося отримати список роздільних здатностей екрану!");
            return;
        }

        if (engDropdown != null) engDropdown.ClearOptions();
        if (ukrDropdown != null) ukrDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // Знаходимо поточну роздільну здатність екрану
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResIndex = i;
            }
        }

        if (engDropdown != null) engDropdown.AddOptions(options);
        if (ukrDropdown != null) ukrDropdown.AddOptions(options);

        // ЧИТАЄМО ЗБЕРЕЖЕНЕ (якщо немає - беремо поточне)
        int savedIndex = PlayerPrefs.GetInt("SelectedRes", currentResIndex);

        // ВАЖЛИВО: Перевірка, щоб індекс не виходив за межі масиву (це викликало помилку 41)
        if (savedIndex >= resolutions.Length) savedIndex = currentResIndex;

        if (engDropdown != null) engDropdown.value = savedIndex;
        if (ukrDropdown != null) ukrDropdown.value = savedIndex;

        ApplyResolution(savedIndex);
    }

    // Окремий метод суто для застосування (без рекурсії)
    private void ApplyResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;

        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    // Цей метод підключаємо до Dropdown в OnValueChanged
    public void SetResolution(int index)
    {
        ApplyResolution(index);

        // ЗБЕРІГАЄМО В ПАМ'ЯТЬ
        PlayerPrefs.SetInt("SelectedRes", index);
        PlayerPrefs.Save();

        // Синхронізація (без виклику самої себе)
        if (engDropdown != null && engDropdown.value != index) engDropdown.value = index;
        if (ukrDropdown != null && ukrDropdown.value != index) ukrDropdown.value = index;
    }
}