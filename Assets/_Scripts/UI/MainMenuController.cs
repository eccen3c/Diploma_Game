using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MainMenuController : MonoBehaviour
{
    public GameObject singlePlayModal;
    public GameObject multiPlayModal;
    public GameObject settingsModal;
    public GameObject mainMenuButtons;
    public GameObject darkOverlay;
    public NetworkLobbyManager networkLobbyManager;

    public void OnSinglePlayClick()
    {
        if (singlePlayModal != null) singlePlayModal.SetActive(true);
        if (mainMenuButtons != null) mainMenuButtons.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(true);
    }

    public void OnMultiPlayClick()
    {
        if (multiPlayModal != null) multiPlayModal.SetActive(true);
        if (mainMenuButtons != null) mainMenuButtons.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(true);
    }

    public void OnSettingsClick()
    {
        if (settingsModal != null) settingsModal.SetActive(true);
        if (mainMenuButtons != null) mainMenuButtons.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(true);
    }

    public void CloseModal()
    {
        if (singlePlayModal != null) singlePlayModal.SetActive(false);
        if (multiPlayModal != null) multiPlayModal.SetActive(false);
        if (settingsModal != null) settingsModal.SetActive(false);
        if (mainMenuButtons != null) mainMenuButtons.SetActive(true);
        if (darkOverlay != null) darkOverlay.SetActive(false);

        if (networkLobbyManager != null)
        {
            if (Photon.Pun.PhotonNetwork.InRoom)
                Photon.Pun.PhotonNetwork.LeaveRoom();
            networkLobbyManager.ShowPanel(networkLobbyManager.panelChoice);
        }
    }

    public void PlayGame()
    {
        GameSession.mode = GameMode.LocalMulti;
        SceneManager.LoadScene("GameScene");
    }

    public void PlaySolo_Easy()
    {
        GameSession.mode = GameMode.SoloVsBot;
        GameSession.difficulty = BotDifficulty.Easy;
        SceneManager.LoadScene("GameScene");
    }

    public void PlaySolo_Medium()
    {
        GameSession.mode = GameMode.SoloVsBot;
        GameSession.difficulty = BotDifficulty.Medium;
        SceneManager.LoadScene("GameScene");
    }

    public void PlaySolo_Hard()
    {
        GameSession.mode = GameMode.SoloVsBot;
        GameSession.difficulty = BotDifficulty.Hard;
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
