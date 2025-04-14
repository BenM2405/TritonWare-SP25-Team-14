using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject gameRoot;
    public GameObject endless;
    public PuzzleManager puzzleManager;

    public void Endless() {
        Debug.Log("Called Endless");
        endless.SetActive(true);
    }

    public void Play2x2()
    {
        Debug.Log("Play2x2() called");
        titleUI.SetActive(false);
        gameRoot.SetActive(true);
        puzzleManager.ConfigureGrid(2,2);
    }

    public void Play3x3()
    {
        Debug.Log("Play3x3() called");
        titleUI.SetActive(false);
        gameRoot.SetActive(true);
        puzzleManager.ConfigureGrid(3,3);
    }

    void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
