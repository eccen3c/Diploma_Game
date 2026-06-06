using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Photon.Pun;

public class GameManager : MonoBehaviour
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

        if (Input.GetKeyDown(KeyCode.Space) && !isGameOver)
            DebugCrippleRandomBase();
    }

    void Start()
    {
        // 1. ������� �����
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        // 2. ��������� �����ު�� ���� ��� ����Ҳ �����
        // �� ����� PlayerPrefs �� �� ����, �� �� �������� �����
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
            // ���������, ��� ������� ���� �� ����
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
        Time.timeScale = 0;
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel) howToPlayPanel.SetActive(false);
        Time.timeScale = SpeedController.instance != null ? SpeedController.instance.GetCurrentSpeed() : 1f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        GameSession.mode = GameMode.LocalMulti;
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        if (GameSession.mode != GameMode.OnlineMulti)
            GameSession.mode = GameMode.LocalMulti;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver(string loserTag)
    {
        if (isGameOver) return;

        isGameOver = true;
        if (gameOverPanel) gameOverPanel.SetActive(true);

        // ���������, �� ������ ������� ����������� ��� ��������
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

    void DebugCrippleRandomBase()
    {
        BaseController[] bases = FindObjectsOfType<BaseController>();
        if (bases.Length == 0) return;
        bases[Random.Range(0, bases.Length)].SetToOneHp();
    }

    private IEnumerator UpdateLanguageNextFrame()
    {
        yield return null;
        FindObjectOfType<LanguageSpritesManager>()?.RefreshLanguage();
    }
}