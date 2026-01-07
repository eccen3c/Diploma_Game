using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Экономика")]
    public int gold = 100;
    public int passiveIncome = 5;
    public float incomeRate = 1f;

    [Header("UI")]
    public TextMeshProUGUI goldText;
    public GameObject gameOverPanel; // Панель проигрыша
    public GameObject pausePanel;    // Панель паузы (НОВОЕ)
    public TextMeshProUGUI resultText;

    // Переменная, чтобы знать, на паузе мы или нет
    private bool isPaused = false;

    void Awake() { instance = this; }

    void Start()
    {
        // Скрываем все панели при старте
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdateUI();
        InvokeRepeating("AddPassiveGold", 1f, incomeRate);

        // Обязательно запускаем время (вдруг вышли из паузы)
        Time.timeScale = 1;
    }

    void AddPassiveGold()
    {
        gold += passiveIncome;
        UpdateUI();
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold: " + gold;
    }

    // --- Управление Игрой ---

    public void TogglePause()
    {
        isPaused = !isPaused; // Переключаем (было true стало false и наоборот)

        if (isPaused)
        {
            Time.timeScale = 0; // Время стоп
            pausePanel.SetActive(true); // Показать меню
        }
        else
        {
            Time.timeScale = 1; // Время пошло
            pausePanel.SetActive(false); // Скрыть меню
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1; // Важно вернуть время перед выходом!
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGame(bool playerWon)
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;

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