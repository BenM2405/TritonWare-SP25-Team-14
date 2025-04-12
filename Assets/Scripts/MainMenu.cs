using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject gameRoot;

    public void PlayGame()
    {
        Debug.Log("PlayGame() called");
        titleUI.SetActive(false);
        gameRoot.SetActive(true);
    }

    void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
