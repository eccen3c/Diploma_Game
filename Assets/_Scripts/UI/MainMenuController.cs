using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject singlePlayModal;
    public GameObject multiPlayModal;
    public GameObject settingsModal;
    public GameObject mainMenuButtons;
    public GameObject darkOverlay;

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
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
