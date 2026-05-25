using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
// Додаємо Photon
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks // Змінили для підтримки Photon
{
    public static GameManager instance;

    public bool isGameOver = false;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject howToPlayPanel;
    public TextMeshProUGUI resultText;

    public bool isPaused = false;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        bool anyPanelOpen = (settingsPanel != null && settingsPanel.activeSelf)
                         || (howToPlayPanel != null && howToPlayPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver && !anyPanelOpen)
            TogglePause();
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        Invoke("ApplyInitialLanguage", 0.05f);

        Time.timeScale = 1;
    }

    private void ApplyInitialLanguage()
    {
        FindObjectOfType<LanguageSpritesManager>()?.RefreshLanguage();
    }

    public void TogglePause()
    {
        // В онлайні ставити гру на паузу (зупиняти час) не можна, бо мережа відвалиться.
        // Тому просто показуємо панель паузи індивідуально для кожного гравця.
        if (PhotonNetwork.IsConnected)
        {
            isPaused = !isPaused;
            if (pausePanel) pausePanel.SetActive(isPaused);
            if (isPaused) StartCoroutine(UpdateLanguageNextFrame());
            return;
        }

        // СТАРИЙ ЛОКАЛЬНИЙ РЕЖИМ (Працює як раніше з Time.timeScale)
        isPaused = !isPaused;

        if (isPaused)
        {
            if (pausePanel) pausePanel.SetActive(true);
            StartCoroutine(UpdateLanguageNextFrame());
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = SpeedController.instance != null ? SpeedController.instance.GetCurrentSpeed() : 1f;
            if (pausePanel) pausePanel.SetActive(false);
        }
    }

    public void OpenSettings()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void OpenHowToPlay()
    {
        if (howToPlayPanel) howToPlayPanel.SetActive(true);
        if (!PhotonNetwork.IsConnected) Time.timeScale = 0; // Зупиняємо час тільки в локалці
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel) howToPlayPanel.SetActive(false);
        if (!PhotonNetwork.IsConnected)
            Time.timeScale = SpeedController.instance != null ? SpeedController.instance.GetCurrentSpeed() : 1f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom(); // Правильно виходимо з кімнати Photon
        }
        SceneManager.LoadScene("MainMenu");
    }

    // Перезапуск гри тепер працює для обох гравців в онлайні
    public void RestartGame()
    {
        Time.timeScale = 1;
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient) // Тільки Хост має право перезавантажити сцену
            {
                PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Мережевий GameOver. Коли викликається, Фотон синхронізує фінал на обох комп'ютерах.
    public void GameOver(string loserTag)
    {
        if (isGameOver) return;

        if (PhotonNetwork.IsConnected)
        {
            // Викликаємо RPC функцію на всіх ПК в кімнаті
            photonView.RPC("RPC_GameOver", RpcTarget.All, loserTag);
        }
        else
        {
            LocalGameOver(loserTag);
        }
    }

    [PunRPC]
    void RPC_GameOver(string loserTag)
    {
        LocalGameOver(loserTag);
    }

    // Твоя оригінальна логіка GameOver
    private void LocalGameOver(string loserTag)
    {
        isGameOver = true;
        if (gameOverPanel) gameOverPanel.SetActive(true);

        StartCoroutine(UpdateLanguageNextFrame());

        Time.timeScale = 0;

        if (resultText != null)
        {
            bool isUkr = PlayerPrefs.GetInt("IsUkrainian", 0) == 1;

            if (loserTag == "Player")
            {
                resultText.text = isUkr ? "Червоні перемогли!!!" : "Red win!!!";
                resultText.color = new Color(0.9f, 0.2f, 0.2f);
            }
            else
            {
                resultText.text = isUkr ? "Сині перемогли!!!" : "Blue win!!!";
                resultText.color = new Color(0.2f, 0.5f, 1f);
            }
        }
    }

    private IEnumerator UpdateLanguageNextFrame()
    {
        yield return null;
        FindObjectOfType<LanguageSpritesManager>()?.RefreshLanguage();
    }
}