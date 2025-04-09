using System.ComponentModel;
using UnityEngine;

public class AudioSettings : MonoBehaviour
{
    private FMOD.Studio.EventInstance sfxTestEvent;
    private FMOD.Studio.EventInstance musicTestEvent;

    private FMOD.Studio.Bus musicBus;
    private FMOD.Studio.Bus sfxBus;
    private FMOD.Studio.Bus masterBus;

    private float musicVolume = 0.5f;
    private float sfxVolume = 0.5f;
    private float masterVolume = 1f;

    private float testMusicTimeoutTimer = 5.0f;

    public bool DebugMode;

    void Awake()
    {
        masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/music");
        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/sfx");

        sfxTestEvent = FMODUnity.RuntimeManager.CreateInstance("event:/sfx/test_sfx");
        musicTestEvent = FMODUnity.RuntimeManager.CreateInstance("event:/music/test_music");
    }

    void Update()
    {
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(sfxVolume);
        masterBus.setVolume(masterVolume);

        if (testMusicTimeoutTimer > 0f)
        {
            testMusicTimeoutTimer -= Time.deltaTime;
        }

        if (checkTestSoundPlaying(musicTestEvent) && testMusicTimeoutTimer <= 0f)
        {
            musicTestEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void SetMasterVolume(float newMasterVolume)
    {
        masterVolume = newMasterVolume;

        if (DebugMode) { Debug.Log("Master Volume: " + masterVolume); }
    }

    public void SetMusicVolume(float newMusicVolume)
    {
        musicVolume = newMusicVolume;
        testMusicTimeoutTimer = 5.0f;

        if (!checkTestSoundPlaying(musicTestEvent))
        {
            musicTestEvent.start();
        }

        if (DebugMode) { Debug.Log("Music Volume: " + musicVolume); }
    }

    public void SetSFXVolume(float newSFXVolume)
    {
        sfxVolume = newSFXVolume;

        if (!checkTestSoundPlaying(sfxTestEvent))
        {
            sfxTestEvent.start();
        }

        if (DebugMode) { Debug.Log("SFX Volume: " + sfxVolume); }
    }

    // checks if the given test sound event instance is playing
    // returns true if it is playing, false if not
    private bool checkTestSoundPlaying(FMOD.Studio.EventInstance eventInstance)
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;
        eventInstance.getPlaybackState(out playbackState);
        if (playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
