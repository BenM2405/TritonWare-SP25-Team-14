using System;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject endless;
    public GameObject storyCanvas;
    public DynamicLevelLoader dynamicLevelLoader;
    private String titleMusicPath = "event:/music/title_music";
    private EventInstance titleMusicEventInstance;

    void Start()
    {
        titleMusicEventInstance = FMODUnity.RuntimeManager.CreateInstance(titleMusicPath);
        titleMusicEventInstance.start();
    }
    public void Story()
    {
        storyCanvas.SetActive(true);
        dynamicLevelLoader.RegenerateButtons();
    }

    public void Endless()
    {
        Debug.Log("Called Endless");
        endless.SetActive(true);
    }

    public void Play2x2()
    {
        Debug.Log("Play2x2() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 2;
        GameConfig.GridHeight = 2;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void Play3x3()
    {
        Debug.Log("Play3x3() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 3;
        GameConfig.GridHeight = 3;
        SceneManager.LoadScene("PuzzleScene");
    }

    void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void stopTitleMusic()
    {
        titleMusicEventInstance.stop(STOP_MODE.ALLOWFADEOUT);
    }
}
