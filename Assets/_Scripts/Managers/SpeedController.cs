using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedController : MonoBehaviour
{
    public static SpeedController instance;

    [Header("UI")]
    public Button speedUpButton;
    public Button speedDownButton;
    public TextMeshProUGUI speedText;

    private readonly float[] speeds = { 0.25f, 0.5f, 0.75f, 1f, 2f, 4f };
    private int speedIndex = 3;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        speedUpButton.onClick.AddListener(SpeedUp);
        speedDownButton.onClick.AddListener(SpeedDown);
        UpdateUI();
    }

    public void SpeedUp()
    {
        if (speedIndex < speeds.Length - 1)
        {
            speedIndex++;
            ApplySpeed();
        }
    }

    public void SpeedDown()
    {
        if (speedIndex > 0)
        {
            speedIndex--;
            ApplySpeed();
        }
    }

    private void ApplySpeed()
    {
        var gm = GameManager.instance;
        bool blocked = gm.isPaused || gm.isGameOver
            || (gm.howToPlayPanel != null && gm.howToPlayPanel.activeSelf);
        if (!blocked)
            Time.timeScale = speeds[speedIndex];
        UpdateUI();
    }

    public float GetCurrentSpeed() => speeds[speedIndex];

    public void ResetSpeed()
    {
        speedIndex = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (speedText != null)
            speedText.text = speeds[speedIndex] + "x";

        if (speedDownButton != null)
            speedDownButton.interactable = speedIndex > 0;

        if (speedUpButton != null)
            speedUpButton.interactable = speedIndex < speeds.Length - 1;
    }
}
