using UnityEngine;
using UnityEngine.SceneManagement; // Библиотека для смены сцен

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        // Загружаем сцену с игрой (важно, чтобы имя совпадало!)
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Debug.Log("Выход из игры!");
        Application.Quit(); // Закрывает игру (работает только в готовом билде, не в редакторе)
    }
}