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

    // --- ЛОГИКА КОНЦА ИГРЫ (ОБНОВЛЕНА) ---

    // Теперь метод называется GameOver и принимает тег проигравшего
    public void GameOver(string loserTag)
    {
        if (isGameOver) return; // Чтобы не вызывалось дважды

        isGameOver = true;

        // Включаем панель
        if (gameOverPanel) gameOverPanel.SetActive(true);

        // Останавливаем время
        Time.timeScale = 0;

        // Пишем результат
        if (resultText != null)
        {
            // Если проиграл Игрок ("Player") -> Значит поражение
            if (loserTag == "Player")
            {
                resultText.text = "DEFEAT";
                resultText.color = Color.red;
            }
            // Если проиграл Враг ("Enemy") -> Значит победа
            else
            {
                resultText.text = "VICTORY!";
                resultText.color = Color.green;
            }
        }
    }
}