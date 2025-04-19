using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject endless;
    public GameObject storyCanvas;
    public GameObject mainButtons;
    public GameObject backButton;
    public DynamicLevelLoader dynamicLevelLoader;
    private String titleMusicPath = "event:/music/title_speech_music";
    private string noteSwapSFXPath = "event:/sfx/puzzle/note_swap";
    private EventInstance titleMusicEventInstance;

    void Start()
    {
        titleMusicEventInstance = FMODUnity.RuntimeManager.CreateInstance(titleMusicPath);
        titleMusicEventInstance.start();
        if (GameConfig.openStoryCanvas)
        {
            GameConfig.openStoryCanvas = false;
            Story(); // starts level select
        }
    }
    public void Story()
    {
        storyCanvas.SetActive(true);
        mainButtons.SetActive(false);
        backButton.SetActive(true);
        dynamicLevelLoader.RegenerateButtons();
    }

    public void Endless()
    {
        Debug.Log("Called Endless");
        endless.SetActive(true);
        backButton.SetActive(true);
        mainButtons.SetActive(false);
    }

    public void Back()
    {
        mainButtons.SetActive(true);
        storyCanvas.SetActive(false);
        backButton.SetActive(false);
        endless.SetActive(false);
        GameConfig.isEndlessMode = false;
    }

    public void Play2x2()
    {
        Debug.Log("Play2x2() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 2;
        GameConfig.GridHeight = 2;
        GameConfig.isEndlessMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void Play3x3()
    {
        Debug.Log("Play3x3() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 3;
        GameConfig.GridHeight = 3;
        GameConfig.isEndlessMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }
    
    public void Play4x4()
    {
        Debug.Log("Play4x4() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 4;
        GameConfig.GridHeight = 4;
        GameConfig.isEndlessMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void Play5x5()
    {
        Debug.Log("Play5x5() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 5;
        GameConfig.GridHeight = 5;
        GameConfig.isEndlessMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void Play6x6()
    {
        Debug.Log("Play4x4() called");
        stopTitleMusic();
        GameConfig.isStoryMode = false;
        GameConfig.GridWidth = 6;
        GameConfig.GridHeight = 6;
        GameConfig.isEndlessMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void Options()
    {
        RuntimeManager.PlayOneShot(noteSwapSFXPath);
    }
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void stopTitleMusic()
    {
        titleMusicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
