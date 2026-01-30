using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameOver = false;

    [Header("UI Панели")]
    public GameObject gameOverPanel; // Панель проигрыша
    public GameObject pausePanel;    // Панель паузы
    public TextMeshProUGUI resultText;

    // Переменная, чтобы знать, на паузе мы или нет
    private bool isPaused = false;

    void Awake() { instance = this; }

    void Start()
    {
        // Скрываем панели при старте
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // ВАЖНО: Мы удалили отсюда start passive income, 
        // так как теперь деньгами управляет GameLoopManager.

        Time.timeScale = 1;
    }

    // --- Управление Системой (Пауза, Меню, Рестарт) ---

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0; // Стоп
            if (pausePanel) pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1; // Играем
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

    public void EndGame(bool playerWon)
    {
        if (isGameOver) return; // Чтобы не вызывалось дважды

        isGameOver = true;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0; // Останавливаем игру

        if (resultText != null)
        {
            if (playerWon)
            {
                resultText.text = "YOU WIN!";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = "DEFEAT";
                resultText.color = Color.red;
            }
        }
    }
}