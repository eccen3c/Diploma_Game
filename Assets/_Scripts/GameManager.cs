using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameOver = false;

    [Header("UI Панели")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public TextMeshProUGUI resultText;

    private bool isPaused = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 1. Сховуємо панелі
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // 2. ПРИМУСОВО ОНОВЛЮЄМО МОВУ ПРИ СТАРТІ СЦЕНИ
        // Це зчитає PlayerPrefs ще до того, як ти натиснеш паузу
        Invoke("ApplyInitialLanguage", 0.05f);

        Time.timeScale = 1;
    }

    private void ApplyInitialLanguage()
    {
        FindObjectOfType<LanguageSpritesManager>()?.RefreshLanguage();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            if (pausePanel) pausePanel.SetActive(true);
            // Оновлюємо, щоб галочка була на місці
            StartCoroutine(UpdateLanguageNextFrame());
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            if (pausePanel) pausePanel.SetActive(false);
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver(string loserTag)
    {
        if (isGameOver) return;

        isGameOver = true;
        if (gameOverPanel) gameOverPanel.SetActive(true);

        // Гарантуємо, що кнопки стануть українськими при програші
        StartCoroutine(UpdateLanguageNextFrame());

        Time.timeScale = 0;

        if (resultText != null)
        {
            bool isUkr = PlayerPrefs.GetInt("IsUkrainian", 0) == 1;

            if (loserTag == "Player")
            {
                resultText.text = isUkr ? "ПОРАЗКА" : "DEFEAT";
                resultText.color = Color.red;
            }
            else
            {
                resultText.text = isUkr ? "ПЕРЕМОГА!" : "VICTORY!";
                resultText.color = Color.green;
            }
        }
    }

    private IEnumerator UpdateLanguageNextFrame()
    {
        yield return null;
        FindObjectOfType<LanguageSpritesManager>()?.RefreshLanguage();
    }
}