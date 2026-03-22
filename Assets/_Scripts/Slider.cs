using UnityEngine;
using UnityEngine.UI;

public class PixelSlider : MonoBehaviour
{
    public AudioSource musicSource;
    public Image backgroundDisplay; // Твій об'єкт Background зі слайдера
    public Sprite[] frames;         // Сюди закинь 5 кадрів (Frame_0...Frame_4)

    public void OnSliderChanged(float value)
    {
        // 1. Встановлюємо гучність (value у слайдера за замовчуванням 0...1)
        if (musicSource != null)
            musicSource.volume = value;

        // 2. Логіка вибору кадру:
        // frames.Length у нас 5. Value * 4 дасть нам індекси від 0 до 4.
        int index = Mathf.RoundToInt(value * (frames.Length - 1));

        // 3. Міняємо картинку
        if (backgroundDisplay != null && frames.Length > 0)
        {
            backgroundDisplay.sprite = frames[index];
        }
    }
}