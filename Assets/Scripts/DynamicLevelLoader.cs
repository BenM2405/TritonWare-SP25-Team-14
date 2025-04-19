using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using FMOD.Studio;
using System.Collections.Generic;

[System.Serializable]
public class LevelMetadata
{
    public int width;
    public int height;
    public List<string> playerSymbols;
    public List<string> targetSymbols;
    public int par;
}

public class DynamicLevelLoader : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    [SerializeField] GameObject menuManager;

    void Start()
    {
        LoadLevelButtons();
    }

    void LoadLevelButtons()
    {
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");

        foreach (TextAsset levelFile in levelFiles)
        {
            LevelMetadata metadata = JsonUtility.FromJson<LevelMetadata>(levelFile.text);
            string buttonLabel = levelFile.name;

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            btnObj.name = $"Btn_{levelFile.name}";
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = buttonLabel;

            string levelName = levelFile.name;
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayStoryLevel(levelName);
            });
        }
    }

    public void RegenerateButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        LoadLevelButtons();
    }

    public void PlayStoryLevel(string levelName)
    {
        menuManager.GetComponent<MainMenu>().stopTitleMusic();
        LevelLoader.Instance.levelToLoad = levelName;
        GameConfig.isStoryMode = true;
        SceneManager.LoadScene("DialogueScene");
    }
}
