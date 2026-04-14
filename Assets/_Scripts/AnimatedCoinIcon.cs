using UnityEngine;
using UnityEngine.UI;

public class AnimatedCoinIcon : MonoBehaviour
{
    // Підключи сюди всі кадри анімації по порядку!
    public Sprite[] coinFrames;
    // Швидкість анімації
    public float framesPerSecond = 10f;

    private Image image;
    private int currentFrame;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
        if (coinFrames == null || coinFrames.Length == 0)
        {
            Debug.LogWarning("Кадри анімації монетки не призначені у AnimatedCoinIcon!");
            enabled = false;
        }
    }

    void Update()
    {
        if (coinFrames == null || coinFrames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / framesPerSecond)
        {
            timer -= 1f / framesPerSecond;
            currentFrame = (currentFrame + 1) % coinFrames.Length;
            image.sprite = coinFrames[currentFrame];
        }
    }
}