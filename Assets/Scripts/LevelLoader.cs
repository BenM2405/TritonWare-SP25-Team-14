using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    public string levelToLoad;
    private EventInstance backgroundMusicEvent;
    public string musicPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(string levelName)
    {
        levelToLoad = levelName;
        GameConfig.isStoryMode = true;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void SetMusic(string musicPath)
    {
        this.musicPath = musicPath;
        backgroundMusicEvent = RuntimeManager.CreateInstance(musicPath);
    }

    public void StartMusic()
    {
        Debug.Log("Starting background music!");
        backgroundMusicEvent.start();
    }

    public void StopMusic()
    {
        Debug.Log("Stopping background music!");
        backgroundMusicEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
